
import React, { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';

import { GameSettings } from '@/types/gameTypes';
import { DEFAULT_BOARD_SIZE, DEFAULT_PLAYER_COUNT } from '@/lib/constants';
import { hasSavedGame } from '@/features/storage/utils/storageUtils';
import { saveSettings, loadSettings } from '@/features/storage/utils/storageUtils';
import { hasCurrentGame, getCurrentGameId } from '@/features/storage/utils/gameIdStorage';
import { useAuth } from '@/features/auth/context/AuthContext';
import  {gameApi } from '@/lib/gameApi';
import { BotDifficulty,  } from '@/lib/utils';
import { saveCurrentGameId } from '@/features/storage/utils/gameIdStorage';



export function useHome() {
    const router = useRouter();
  const { isAuthenticated } = useAuth();
  const [showSettings, setShowSettings] = useState(false);
  const [showAbout, setShowAbout] = useState(false);
  const [showBotDifficulty, setShowBotDifficulty] = useState(false);
  const [hasSave, setHasSave] = useState(false);
  const [isCreatingGame, setIsCreatingGame] = useState(false);
  const [settings, setSettings] = useState<GameSettings>({
    playerCount: DEFAULT_PLAYER_COUNT,
    boardSize: DEFAULT_BOARD_SIZE,
    theme: 'light',
    showValidPaths: false,
  });

  useEffect(() => {
    // Load settings from localStorage
    const savedSettings = loadSettings();
    if (savedSettings) {
      setSettings(savedSettings);
      applyTheme(savedSettings.theme);
    }

    // Check for saved game (offline or online)
    setHasSave(hasSavedGame() || hasCurrentGame());
  }, []);

  const applyTheme = (theme: 'light' | 'dark') => {
    if (theme === 'dark') {
      document.documentElement.classList.add('dark');
    } else {
      document.documentElement.classList.remove('dark');
    }
  };

  const handleSettingsChange = (newSettings: Partial<GameSettings>) => {
    const updated = { ...settings, ...newSettings };
    setSettings(updated);
    saveSettings(updated);
    
    if (newSettings.theme) {
      applyTheme(newSettings.theme);
    }
  };

  const startNewGame = () => {
    // Show bot difficulty selection
    setShowBotDifficulty(true);
  };

  const handleBotDifficultySelect = async (difficulty: BotDifficulty) => {
    setShowBotDifficulty(false);
    setIsCreatingGame(true);

    try {
      // console.log('Creating bot game with difficulty:', difficulty);
      const response = await gameApi.createBotGame({
        settings: {
          playerCount: 2, // Bot games are always 2 players
          boardSize: settings.boardSize,
          theme: settings.theme,
          showValidPaths: settings.showValidPaths,
        },
        botDifficulty: difficulty,
        botPlayerIndex: 1,
      });

      // Save game ID to localStorage for "continue game" feature
      saveCurrentGameId(response.gameId);

      // Navigate to game with the game ID
      router.push(`/game?gameId=${response.gameId}`);
    } catch (error: any) {
      console.error('Error creating bot game:', error);
      const errorMessage = error?.message || 'Failed to create game. Please make sure you are signed in.';
      alert(errorMessage);
      setIsCreatingGame(false);
    }
  };

  const continueSavedGame = () => {
    // Check if there's an online game first
    const currentGameId = getCurrentGameId();
    if (currentGameId) {
      router.push(`/game?gameId=${currentGameId}`);
    } else {
      // Fall back to offline saved game
      router.push('/game?load=true');
    }
  };

  return {
    isAuthenticated,
    showSettings,
    setShowSettings,
    showAbout,
    setShowAbout,
    showBotDifficulty,
    setShowBotDifficulty,
    hasSave,
    setHasSave,
    isCreatingGame,
    setIsCreatingGame,
    settings,
    setSettings,
    startNewGame,
    handleBotDifficultySelect,
    continueSavedGame,
    handleSettingsChange,
  };
}