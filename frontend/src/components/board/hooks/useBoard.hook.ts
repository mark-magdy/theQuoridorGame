import { useMemo, useState } from 'react';
import { Position, Wall as WallType, GameState } from '@/types/gameTypes';
import { canPlaceWall, getValidPawnMoves } from '../utils/boardUtils';

interface UseBoardProps {
  gameState: GameState;
  onPawnMove: (position: Position) => void;
  onWallPlace: (wall: WallType) => Promise<boolean>;
  isPlacingWall: boolean;
  setIsPlacingWall: (value: boolean) => void;
  setPlacementError: (error: string) => void;
}

export function useBoard({
  gameState,
  onPawnMove,
  onWallPlace,
  isPlacingWall,
  setIsPlacingWall,
  setPlacementError,
}: UseBoardProps) {
  const [selectedCell, setSelectedCell] = useState<Position | null>(null);
  const [hoveredWallPosition, setHoveredWallPosition] = useState<{ position: Position; isHorizontal: boolean } | null>(null);

  const boardSize = gameState.boardSize;
  const currentPlayer = gameState.players[gameState.currentPlayerIndex];

  // Generate all cells on the board
  const cells = useMemo(() => {
    const cellArray: Position[] = [];
    for (let row = 0; row < boardSize; row++) {
      for (let col = 0; col < boardSize; col++) {
        cellArray.push({ row, col });
      }
    }
    return cellArray;
  }, [boardSize]);

  // Calculate valid moves for the current player
  const validMoves = useMemo(() => {
    if (!currentPlayer || isPlacingWall || gameState.gameStatus !== 'Playing') {
      return [];
    }
    return getValidPawnMoves(
      currentPlayer,
      gameState.players,
      gameState.walls,
      boardSize
    );
  }, [currentPlayer, gameState.players, gameState.walls, boardSize, isPlacingWall, gameState.gameStatus]);

  const isValidMove = (pos: Position) => {
    return validMoves.some(m => m.row === pos.row && m.col === pos.col);
  };

  const isSelected = (pos: Position) => {
    return selectedCell?.row === pos.row && selectedCell?.col === pos.col;
  };

  const handleCellClick = (position: Position) => {
    if (isPlacingWall || gameState.gameStatus !== 'Playing') return;

    // Check if clicking on current player's pawn
    if (currentPlayer && position.row === currentPlayer.position.row && position.col === currentPlayer.position.col) {
      setSelectedCell(position);
      return;
    }

    // Check if it's a valid move
    if (isValidMove(position)) {
      onPawnMove(position);
      setSelectedCell(null);
      
    } else {
      setSelectedCell(null);
    }
  };

  const handleWallHover = (row: number, col: number, isHorizontal: boolean) => {
    if (!isPlacingWall || !currentPlayer || currentPlayer.wallsRemaining <= 0) return;
    if ((isHorizontal && row === 0) || (!isHorizontal && col === 0)) return;
    setHoveredWallPosition({ position: { row, col }, isHorizontal });
    setPlacementError('');
  };

  const handleWallClick = async (row: number, col: number, isHorizontal: boolean) => {
    if (!isPlacingWall || !currentPlayer || currentPlayer.wallsRemaining <= 0) return;

    const wall: WallType = {
      position: { row, col },
      isHorizontal,
    };

    // Check basic validity first
    if (!canPlaceWall(wall, gameState.walls, boardSize)) {
      setPlacementError('Invalid wall placement! Check rules.');
      setTimeout(() => setPlacementError(''), 2000);
      return;
    }

    const success = await onWallPlace(wall);
    
    if (success) {
      setHoveredWallPosition(null);
      setIsPlacingWall(false);
      setPlacementError('');
    } else {
      setPlacementError('Invalid wall placement! Blocks path to goal.');
      setTimeout(() => setPlacementError(''), 2000);
    }
  };

  const isWallValid = (wall: WallType): boolean => {
    return canPlaceWall(wall, gameState.walls, boardSize);
  };

  return {
    cells,
    validMoves,
    selectedCell,
    hoveredWallPosition,
    isValidMove,
    isSelected,
    handleCellClick,
    handleWallHover,
    handleWallClick,
    isWallValid,
    setHoveredWallPosition,
  };
}