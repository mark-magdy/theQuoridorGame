'use client';

import React from 'react';
import { GameSettings } from '@/types/gameTypes';
import Toggle from '@/components/common/Toggle';
import Button from '@/components/common/Button';
import { BOARD_SIZES, PLAYER_COUNTS } from '@/lib/constants';

interface SettingsMenuProps {
  settings: GameSettings;
  onSettingsChange: (settings: Partial<GameSettings>) => void;
  onClose: () => void;
  isInGame?: boolean;
}

const SettingsMenu: React.FC<SettingsMenuProps> = ({
  settings,
  onSettingsChange,
  onClose,
  isInGame = false,
}) => {
  return (
    <div className="space-y-6">
      {/* Theme Toggle */}
      <div className="flex items-center justify-between p-4 bg-gray-50 dark:bg-gray-700 rounded-lg">
        <div>
          <h3 className="font-semibold text-gray-900 dark:text-white">Theme</h3>
          <p className="text-sm text-gray-600 dark:text-gray-400">
            Choose light or dark mode
          </p>
        </div>
        <Toggle
          enabled={settings.theme === 'dark'}
          onChange={(enabled) => onSettingsChange({ theme: enabled ? 'dark' : 'light' })}
          label={settings.theme === 'dark' ? '🌙 Dark' : '☀️ Light'}
        />
      </div>

      {/* Player Count */}
      {!isInGame && (
        <div className="p-4 bg-gray-50 dark:bg-gray-700 rounded-lg">
          <h3 className="font-semibold text-gray-900 dark:text-white mb-3">
            Number of Players
          </h3>
          <div className="grid grid-cols-3 gap-2">
            {PLAYER_COUNTS.map((count) => (
              <button
                key={count}
                onClick={() => onSettingsChange({ playerCount: count })}
                className={`
                  py-2 px-4 rounded-lg font-semibold transition-all
                  ${settings.playerCount === count
                    ? 'bg-blue-600 text-white'
                    : 'bg-white dark:bg-gray-600 text-gray-800 dark:text-white hover:bg-gray-100 dark:hover:bg-gray-500'
                  }
                `}
              >
                {count}
              </button>
            ))}
          </div>
        </div>
      )}

      {/* Board Size */}
      {!isInGame && (
        <div className="p-4 bg-gray-50 dark:bg-gray-700 rounded-lg">
          <h3 className="font-semibold text-gray-900 dark:text-white mb-3">
            Board Size
          </h3>
          <div className="grid grid-cols-3 gap-2">
            {BOARD_SIZES.map((size) => (
              <button
                key={size}
                onClick={() => onSettingsChange({ boardSize: size })}
                className={`
                  py-2 px-4 rounded-lg font-semibold transition-all
                  ${settings.boardSize === size
                    ? 'bg-blue-600 text-white'
                    : 'bg-white dark:bg-gray-600 text-gray-800 dark:text-white hover:bg-gray-100 dark:hover:bg-gray-500'
                  }
                `}
              >
                {size}×{size}
              </button>
            ))}
          </div>
          <p className="text-xs text-gray-600 dark:text-gray-400 mt-2">
            Standard: 9×9 · Compact: 7×7 · Extended: 11×11
          </p>
        </div>
      )}

      {/* Show Valid Paths (Debug) */}
      <div className="flex items-center justify-between p-4 bg-gray-50 dark:bg-gray-700 rounded-lg">
        <div>
          <h3 className="font-semibold text-gray-900 dark:text-white">Debug Mode</h3>
          <p className="text-sm text-gray-600 dark:text-gray-400">
            Show valid paths for debugging
          </p>
        </div>
        <Toggle
          enabled={settings.showValidPaths}
          onChange={(enabled) => onSettingsChange({ showValidPaths: enabled })}
        />
      </div>

      {/* Warning for in-game changes */}
      {isInGame && (
        <div className="p-4 bg-yellow-50 dark:bg-yellow-900 border border-yellow-200 dark:border-yellow-700 rounded-lg">
          <p className="text-sm text-yellow-800 dark:text-yellow-200">
            ⚠️ Some settings can only be changed before starting a new game.
          </p>
        </div>
      )}

      {/* Actions */}
      <div className="flex justify-end gap-3">
        <Button variant="secondary" onClick={onClose}>
          Close
        </Button>
      </div>
    </div>
  );
};

export default SettingsMenu;

