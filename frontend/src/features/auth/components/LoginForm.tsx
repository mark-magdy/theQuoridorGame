'use client';

import React, { useState } from 'react';
import { useAuth } from '@/features/auth/context/AuthContext';
import { ApiError } from '@/lib/apiClient';
import Button from '@/components/common/Button';
import Icon from '@/components/common/Icon';

interface LoginFormProps {
  onSuccess?: () => void;
  onSwitchToRegister?: () => void;
}

export default function LoginForm({ onSuccess, onSwitchToRegister }: LoginFormProps) {
  const { login } = useAuth();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setIsLoading(true);

    try {
      await login({ email, password });
      onSuccess?.();
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.message);
      } else {
        setError('An unexpected error occurred');
      }
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <div>
        <label htmlFor="email" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
          Email
        </label>
        <input
          id="email"
          type="email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          required
          className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
          placeholder="you@example.com"
        />
      </div>

      <div>
        <label htmlFor="password" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
          Password
        </label>
        <input
          id="password"
          type="password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          required
          className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
          placeholder="••••••••"
        />
      </div>

      {error && (
        <div className="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 text-red-700 dark:text-red-400 px-4 py-3 rounded-lg">
          {error}
        </div>
      )}

      <Button
        type="submit"
        variant="primary"
        size="lg"
        disabled={isLoading}
        className="w-full"
      >
        {isLoading ? (
          <>
            <Icon name="loading" size={20} className="inline mr-2 animate-spin" />
            Logging in...
          </>
        ) : (
          <>
            <Icon name="login" size={20} className="inline mr-2" />
            Login
          </>
        )}
      </Button>

      {onSwitchToRegister && (
        <p className="text-center text-sm text-gray-600 dark:text-gray-400">
          Don&apos;t have an account?{' '}
          <button
            type="button"
            onClick={onSwitchToRegister}
            className="text-blue-600 dark:text-blue-400 hover:underline font-medium"
          >
            Register here
          </button>
        </p>
      )}
    </form>
  );
}
