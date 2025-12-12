import { apiClient } from '@/lib/apiClient';
import {
  AuthResponse,
  LoginRequest,
  RegisterRequest,
  UserProfile,
  LeaderboardResponse,
  UserStats,
} from '@/types/authTypes';

export const authApi = {
  /**
   * Register a new user
   */
  register: async (data: RegisterRequest): Promise<AuthResponse> => {
    return apiClient.post<AuthResponse>('/auth/register', data);
  },

  /**
   * Login with email and password
   */
  login: async (data: LoginRequest): Promise<AuthResponse> => {
    return apiClient.post<AuthResponse>('/auth/login', data);
  },

  /**
   * Get current user's profile
   */
  getMyProfile: async (): Promise<UserProfile> => {
    return apiClient.get<UserProfile>('/users/me');
  },

  /**
   * Get a specific user's profile
   */
  getUserProfile: async (userId: string): Promise<UserProfile> => {
    return apiClient.get<UserProfile>(`/users/${userId}`);
  },

  /**
   * Get user statistics
   */
  getUserStats: async (userId: string): Promise<UserStats> => {
    return apiClient.get<UserStats>(`/users/${userId}/stats`);
  },

  /**
   * Get leaderboard
   */
  getLeaderboard: async (limit = 50, offset = 0): Promise<LeaderboardResponse> => {
    return apiClient.get<LeaderboardResponse>(
      `/users/leaderboard?limit=${limit}&offset=${offset}`
    );
  },

  /**
   * Authenticate with Google
   */
  googleAuth: async (googleToken: string): Promise<AuthResponse> => {
    return apiClient.post<AuthResponse>('/auth/google', { googleToken });
  },
};
