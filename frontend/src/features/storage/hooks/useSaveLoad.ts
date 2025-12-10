import { useCallback } from 'react';
import { GameState } from '@/types/gameTypes';
import { saveGameState, loadGameState, clearGameState } from '../utils/storageUtils';

export function useSaveLoad() {
  const saveGame = useCallback((state: GameState) => {
    saveGameState(state);
  }, []);

  const loadGame = useCallback((): GameState | null => {
    return loadGameState();
  }, []);

  const clearSave = useCallback(() => {
    clearGameState();
  }, []);

  return {
    saveGame,
    loadGame,
    clearSave,
  };
}


