export interface User {
  id: string;
  username: string;
  email: string;
  createdAt: string;
}

export interface UserProfile extends User {
  stats: UserStats;
}

export interface UserStats {
  fastestWin: number;
  gamesPlayed:number; 
  gamesWon: number;
  winRate: number;
  totalMoves:number; 
  wallPlaced:number; 
}

export interface AuthResponse {
  user: User;
  token: string;
  expiresAt: string;
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface GoogleAuthRequest {
  googleToken: string;
}

export interface LeaderboardEntry {
  rank: number;
  userId: string;
  username: string;
  wins: number;
  winRate: number;
  totalGames: number;
}

export interface LeaderboardResponse {
  entries: LeaderboardEntry[];
  total: number;
  limit: number;
  offset: number;
}

export interface ErrorResponse {
  message: string;
  errors?: Record<string, string[]>;
}
