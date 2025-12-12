import { GameState, GameSettings, PlayerType, PlayerColor } from '@/types/gameTypes';

export enum BotDifficulty {
  Easy = 0,
  Medium = 1,
  Hard = 2
}

// Map numeric enums from backend to string enums for frontend
 const GameStatusMap: Record<number, 'Menu' | 'Waiting' | 'Playing' | 'Paused' | 'Finished'> = {
  0: 'Menu',
  1: 'Waiting',
  2: 'Playing',
  3: 'Paused',
  4: 'Finished'
};

const PlayerTypeMap: Record<number, PlayerType> = {
  0: 'Human',
  1: 'Bot'
};

const PlayerColorMap: Record<number, PlayerColor> = {
  0: 'player1',
  1: 'player2',
  2: 'player3',
  3: 'player4'
};

// Frontend to backend: Convert string move types to numeric enums
export const MoveTypeToBackend: Record<string, number> = {
  'Pawn': 0,
  'Wall': 1
};

// Transform game state from backend format to frontend format
export function transformGameState(gameState: any): GameState {
  return {
    ...gameState,
    gameStatus: GameStatusMap[gameState.gameStatus] || 'Menu',
    players: gameState.players.map((player: any) => ({
      ...player,
      type: PlayerTypeMap[player.type] || 'Human',
      color: PlayerColorMap[player.color] || 'player1'
    }))
  };
}