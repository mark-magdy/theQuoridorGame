import { GameState, GameSettings } from '@/types/gameTypes';
import { STORAGE_KEYS } from '@/lib/constants';

export function saveGameState(state: GameState): void {
  try {
    const serialized = JSON.stringify(state);
    localStorage.setItem(STORAGE_KEYS.GAME_STATE, serialized);
  } catch (error) {
    console.error('Failed to save game state:', error);
  }
}

export function loadGameState(): GameState | null {
  try {
    const serialized = localStorage.getItem(STORAGE_KEYS.GAME_STATE);
    if (serialized === null) {
      return null;
    }
    return JSON.parse(serialized) as GameState;
  } catch (error) {
    console.error('Failed to load game state:', error);
    return null;
  }
}

export function clearGameState(): void {
  try {
    localStorage.removeItem(STORAGE_KEYS.GAME_STATE);
  } catch (error) {
    console.error('Failed to clear game state:', error);
  }
}

export function saveSettings(settings: GameSettings): void {
  try {
    const serialized = JSON.stringify(settings);
    localStorage.setItem(STORAGE_KEYS.SETTINGS, serialized);
  } catch (error) {
    console.error('Failed to save settings:', error);
  }
}

export function loadSettings(): GameSettings | null {
  try {
    const serialized = localStorage.getItem(STORAGE_KEYS.SETTINGS);
    if (serialized === null) {
      return null;
    }
    return JSON.parse(serialized) as GameSettings;
  } catch (error) {
    console.error('Failed to load settings:', error);
    return null;
  }
}

export function hasSavedGame(): boolean {
  return localStorage.getItem(STORAGE_KEYS.GAME_STATE) !== null;
}


