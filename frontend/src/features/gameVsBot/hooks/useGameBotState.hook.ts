import { useState, useEffect, useCallback, useRef } from 'react';
import { useSearchParams, useRouter } from 'next/navigation';
import { GameState, Position, Wall, Move } from '@/types/gameTypes';
import { gameApi, MakeMoveRequest } from '@/lib/gameApi';
import { BotDifficulty} from "@/lib/utils";
 
const initialGameState: GameState = {
  boardSize: 9,
  players: [],
  currentPlayerIndex: 0,
  walls: [],
  gameStatus: 'Menu',
  winner: null,
  moveHistory: [],
  historyIndex: 0,
};

export function useGameState() {
  const searchParams = useSearchParams();
  const router = useRouter();
  const [gameState, setGameState] = useState<GameState>(initialGameState);
  const [gameId, setGameId] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [isPlayerTurn, setIsPlayerTurn] = useState(true);
  const isProcessingMove = useRef(false);

  // Load game on mount
  useEffect(() => {
    const gameIdFromUrl = searchParams.get('gameId');
    if (gameIdFromUrl) {
      loadGame(gameIdFromUrl);
    } else {
      // If no gameId, redirect to home or create new game
      router.push('/');
    }
  }, [searchParams]);

  // Load existing game from backend
  const loadGame = async (id: string) => {
    try {
      setIsLoading(true);
      setError(null);
      const gameDto = await gameApi.getGame(id);
      setGameId(id);
      setGameState(gameDto.gameState);
      // console.log(">>>> game", gameDto.gameState);
      // Check if it's player's turn
      const currentPlayer = gameDto.gameState.players[gameDto.gameState.currentPlayerIndex];
      setIsPlayerTurn(currentPlayer.type !== 'Bot');
      
      // If it's bot's turn, it should have already moved, but check game status
      if (gameDto.gameState.gameStatus === 'Finished') {
        showGameResult(gameDto.gameState.winner);
      }
    } catch (err: any) {
      setError(err.message || 'Failed to load game');
      console.error('Error loading game:', err);
    } finally {
      setIsLoading(false);
    }
  };

  // Show game result
  const showGameResult = (winnerId: number | null) => {
    if (winnerId === null) return;
    
    const winner = gameState.players.find(p => p.id === winnerId);
    if (winner) {
      const isPlayerWinner = winner.type !== 'Bot';
      setTimeout(() => {
        alert(isPlayerWinner ? '🎉 Congratulations! You won!' : '😔 Bot won! Try again.');
      }, 500);
    }
  };

  // Move pawn to a new position
  const movePawn = useCallback(async (to: Position) => {
    if (!gameId || !isPlayerTurn || isLoading || isProcessingMove.current) return;

    const currentPlayer = gameState.players[gameState.currentPlayerIndex];
    
    const moveRequest: MakeMoveRequest = {move:{
      type: 0, // 0 = Pawn
      playerId: currentPlayer.id,
      timestamp: Date.now(),
      from: currentPlayer.position,
      to: to,
    }};

    try {
      isProcessingMove.current = true;
      setIsLoading(true);
      setError(null);
      
      const response = await gameApi.makeMove(gameId, moveRequest);
      
      if (!response.isValid) {
        setError(response.error || 'Invalid move');
        alert(response.error || 'Invalid move');
        return;
      }

      // Update game state with the response
      if (response.gameState) {
        setGameState(response.gameState);
      }

      // Check if game ended
      if (response.gameEnded && response.winnerId !== undefined) {
        showGameResult(response.winnerId);
      }

      // Bot move is already included in the gameState from backend
      // The backend handles bot's move automatically
      
    } catch (err: any) {
      setError(err.message || 'Failed to make move');
      console.error('Error making move:', err);
      alert(err.message || 'Failed to make move');
    } finally {
      setIsLoading(false);
      isProcessingMove.current = false;
    }
  }, [gameId, gameState, isPlayerTurn, isLoading]);


  // Async wall placement
  const placeWall = async (wall: Wall) : Promise<boolean> => {
    if (!gameId || isProcessingMove.current) return false;

    const currentPlayer = gameState.players[gameState.currentPlayerIndex];
    let canPlaceWall = true ; 

    const moveRequest: MakeMoveRequest = {move:{
      type: 1, // 1 = Wall
      playerId: currentPlayer.id,
      timestamp: Date.now(),
      wall: wall,
    }};

    try {
      isProcessingMove.current = true;
      setIsLoading(true);
      setError(null);
      
      const response = await gameApi.makeMove(gameId, moveRequest);
      
      if (!response.isValid) {
        setError(response.error || 'Invalid wall placement');
        alert(response.error || 'Invalid wall placement - this may block all paths to goal');
        // Reload game state to revert the optimistic update
        if (gameId) await loadGame(gameId);
        canPlaceWall = false;
      }

      // Update game state with the response
      if (response.gameState) {
        setGameState(response.gameState);
      }

      // Check if game ended
      if (response.gameEnded && response.winnerId !== undefined) {
        showGameResult(response.winnerId);
      }

    } catch (err: any) {
      setError(err.message || 'Failed to place wall');
      console.error('Error placing wall:', err);
      alert(err.message || 'Failed to place wall');
      // Reload game state to revert the optimistic update
      if (gameId) await loadGame(gameId);
      canPlaceWall=false;
    } finally {
      setIsLoading(false);
      isProcessingMove.current = false;
    }
    return canPlaceWall;
  };

  // Undo move (not supported with backend game state)
  const undo = useCallback(() => {
    alert('Undo is not available in online games');
  }, []);

  const redo = useCallback(() => {
    alert('Redo is not available in online games');
  }, []);

  // Restart game
  const restart = useCallback(async () => {
    if (!gameId) return;
    
    const confirmed = confirm('Are you sure you want to restart the game? This will create a new game. and delete this one');
    if (!confirmed) return;

    try {
      // Delete current game and redirect to home to create new one
      await gameApi.deleteGame(gameId);
      router.push('/');
    } catch (err: any) {
      console.error('Error restarting game:', err);
      alert('Failed to restart game');
    }
  }, [gameId, router]);

  // // Load state (not applicable for backend-managed games)
  // const loadState = useCallback((state: GameState) => {
  //   alert('Loading saved states is not available in online games');
  // }, []);

  // // Update settings (not applicable for ongoing games)
  // const updateSettings = useCallback((settings: any) => {
  //   alert('Settings cannot be changed during an ongoing game');
  // }, []);

  return {
    gameState,
    movePawn,
    placeWall,
    undo,
    redo,
    restart,
    isLoading,
    error,
    isPlayerTurn,
    gameId,
  };
}