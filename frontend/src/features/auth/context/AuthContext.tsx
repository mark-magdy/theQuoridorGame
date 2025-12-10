'use client';

import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { apiClient } from '@/lib/apiClient';
import { authApi } from '@/features/auth/api/authApi';
import {
  User,
  UserProfile,
  LoginRequest,
  RegisterRequest,
  AuthResponse,
} from '@/types/authTypes';

interface AuthContextType {
  user: User | null;
  profile: UserProfile | null;
  isLoading: boolean;
  isAuthenticated: boolean;
  login: (data: LoginRequest) => Promise<void>;
  register: (data: RegisterRequest) => Promise<void>;
  logout: () => void;
  refreshProfile: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [profile, setProfile] = useState<UserProfile | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    // Check if user is already logged in
    const checkAuth = async () => {
      const token = apiClient.getToken();
      if (token) {
        try {
          const userProfile = await authApi.getMyProfile();
          setUser({
            id: userProfile.id,
            username: userProfile.username,
            email: userProfile.email,
            createdAt: userProfile.createdAt,
          });
          setProfile(userProfile);
        } catch (error) {
          // Token is invalid, clear it
          apiClient.setToken(null);
        }
      }
      setIsLoading(false);
    };

    checkAuth();
  }, []);

  const login = async (data: LoginRequest) => {
    const response: AuthResponse = await authApi.login(data);
    apiClient.setToken(response.token);
    setUser(response.user);
    
    // Fetch full profile
    const userProfile = await authApi.getMyProfile();
    setProfile(userProfile);
  };

  const register = async (data: RegisterRequest) => {
    const response: AuthResponse = await authApi.register(data);
    apiClient.setToken(response.token);
    setUser(response.user);
    
    // Fetch full profile
    const userProfile = await authApi.getMyProfile();
    setProfile(userProfile);
  };

  const logout = () => {
    apiClient.setToken(null);
    setUser(null);
    setProfile(null);
  };

  const refreshProfile = async () => {
    if (user) {
      const userProfile = await authApi.getMyProfile();
      setProfile(userProfile);
    }
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        profile,
        isLoading,
        isAuthenticated: !!user,
        login,
        register,
        logout,
        refreshProfile,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}
