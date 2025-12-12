import { apiClient } from './apiClient';
import { GameState, GameSettings, PlayerType, PlayerColor } from '@/types/gameTypes';

import {BotDifficulty, MoveTypeToBackend , transformGameState } from "./utils";

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
