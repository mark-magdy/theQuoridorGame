'use client';

import React from 'react';
import { useAuth } from '@/features/auth/context/AuthContext';
import Icon from '@/components/common/Icon';

export default function UserProfileCard() {
  const { profile, isLoading } = useAuth();

  if (isLoading || !profile) {
    return null;
  }

  const stats = profile.stats;
  console.log(" profile info: " , profile)
  return (
    <div className="bg-white dark:bg-gray-800 rounded-xl shadow-lg p-6 space-y-4">
      <div className="flex items-center space-x-4">
        <div className="w-16 h-16 bg-gradient-to-br from-blue-500 to-purple-600 rounded-full flex items-center justify-center text-white text-2xl font-bold">
          {profile.username.charAt(0).toUpperCase()}
        </div>
        <div>
          <h3 className="text-xl font-bold text-gray-900 dark:text-white">
            {profile.username}
          </h3>
          <p className="text-sm text-gray-500 dark:text-gray-400">
            {profile.email}
          </p>
        </div>
      </div>

      <div className="border-t border-gray-200 dark:border-gray-700 pt-4">
        <h4 className="text-sm font-semibold text-gray-700 dark:text-gray-300 mb-3">
          Statistics
        </h4>
        <div className="grid grid-cols-2 gap-4">
          <div className="bg-blue-50 dark:bg-blue-900/20 p-3 rounded-lg">
            <div className="flex items-center text-blue-600 dark:text-blue-400 mb-1">
              <Icon name="trophy" size={16} className="mr-1" />
              <span className="text-xs font-medium">Wins</span>
            </div>
            <p className="text-2xl font-bold text-gray-900 dark:text-white">
              {stats.gamesWon}
            </p>
          </div>

          <div className="bg-green-50 dark:bg-green-900/20 p-3 rounded-lg">
            <div className="flex items-center text-green-600 dark:text-green-400 mb-1">
              <Icon name="chart" size={16} className="mr-1" />
              <span className="text-xs font-medium">Win Rate</span>
            </div>
            <p className="text-2xl font-bold text-gray-900 dark:text-white">
              {(stats.winRate).toFixed(2)}%
            </p>
          </div>

          <div className="bg-purple-50 dark:bg-purple-900/20 p-3 rounded-lg">
            <div className="flex items-center text-purple-600 dark:text-purple-400 mb-1">
              <Icon name="fire" size={16} className="mr-1" />
              <span className="text-xs font-medium">Fastest Win in</span>
            </div>
            <p className="text-2xl font-bold text-gray-900 dark:text-white">
              {stats.fastestWin} {stats.fastestWin>1 ? 'Moves' : 'Move'}
            </p>
          </div>

          <div className="bg-orange-50 dark:bg-orange-900/20 p-3 rounded-lg">
            <div className="flex items-center text-orange-600 dark:text-orange-400 mb-1">
              <Icon name="gamepad" size={16} className="mr-1" />
              <span className="text-xs font-medium">Games Played</span>
            </div>
            <p className="text-2xl font-bold text-gray-900 dark:text-white">
              {stats.gamesPlayed}
            </p>
          </div>
        </div>
      </div>

      <div className="text-xs text-gray-500 dark:text-gray-400 text-center pt-2">
        Member since {new Date(profile.createdAt).toLocaleDateString()}
      </div>
    </div>
  );
}
