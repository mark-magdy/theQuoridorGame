import { apiClient } from './apiClient';
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
const MoveTypeToBackend: Record<string, number> = {
  'Pawn': 0,
  'Wall': 1
};

// Transform game state from backend format to frontend format
function transformGameState(gameState: any): GameState {
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

export interface CreateBotGameRequest {
  settings: GameSettings;
  botDifficulty: BotDifficulty;
  botPlayerIndex?: number;
}

export interface CreateBotGameResponse {
  gameId: string;
  gameState: GameState;
}

export interface GameDto {
  id: string;
  gameState: GameState;
  settings: GameSettings;
  status: string;
  createdAt: string;
  startedAt?: string;
  finishedAt?: string;
  isPrivate: boolean;
}

export interface MakeMoveRequest {
    move: {
  type: number; // 0 for Pawn, 1 for Wall
  playerId: number;
  timestamp: number;
  from?: { row: number; col: number };
  to?: { row: number; col: number };
  wall?: {
    position: { row: number; col: number };
    isHorizontal: boolean;
  };
}
}

export interface MakeMoveResponse {
  isValid: boolean;
  gameState?: GameState;
  error?: string;
  botMove?: {
    type: 'Pawn' | 'Wall';
    playerId: number;
    timestamp: number;
    from?: { row: number; col: number };
    to?: { row: number; col: number };
    wall?: {
      position: { row: number; col: number };
      isHorizontal: boolean;
    };
  };
  gameEnded?: boolean;
  winnerId?: number;
}

export interface AvailableMovesResponse {
  validPawnMoves: string[]; // Cell IDs in algebraic notation (e.g., ["a1", "b2"])
  validWallPlacements: string[]; // Wall IDs in algebraic notation (e.g., ["a1h", "b2v"])
}

export const gameApi = {
  async createBotGame(request: CreateBotGameRequest): Promise<CreateBotGameResponse> {
    const response = await apiClient.post<CreateBotGameResponse>('/games/bot', request);
    return {
      ...response,
      gameState: transformGameState(response.gameState)
    };
  },

  async getGame(gameId: string): Promise<GameDto> {
    const response = await apiClient.get<GameDto>(`/games/${gameId}`);
    return {
      ...response,
      gameState: transformGameState(response.gameState)
    };
  },

  async getMyGames(): Promise<GameDto[]> {
    const response = await apiClient.get<GameDto[]>('/games/my-games');
    return response.map(game => ({
      ...game,
      gameState: transformGameState(game.gameState)
    }));
  },

  async getMyFinishedGames(): Promise<GameDto[]> {
    const response = await apiClient.get<GameDto[]>('/games/my-games/finished');
    return response.map(game => ({
      ...game,
      gameState: transformGameState(game.gameState)
    }));
  },

  async getAvailableMoves(gameId: string): Promise<AvailableMovesResponse> {
    return apiClient.get<AvailableMovesResponse>(`/games/${gameId}/available-moves`);
  },

  async makeMove(gameId: string, request: MakeMoveRequest): Promise<MakeMoveResponse> {
    const response = await apiClient.post<MakeMoveResponse>(`/games/${gameId}/moves`, request);
    if (response.gameState) {
      response.gameState = transformGameState(response.gameState);
    }
    return response;
  },

  async deleteGame(gameId: string): Promise<void> {
    return apiClient.delete<void>(`/games/${gameId}`);
  }
};
