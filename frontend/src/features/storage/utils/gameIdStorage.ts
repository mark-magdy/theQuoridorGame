// Store current game ID in localStorage for "continue game" feature
export function saveCurrentGameId(gameId: string): void {
  if (typeof window !== 'undefined') {
    localStorage.setItem('current_game_id', gameId);
    localStorage.setItem('current_game_timestamp', Date.now().toString());
  }
}

export function getCurrentGameId(): string | null {
  if (typeof window !== 'undefined') {
    return localStorage.getItem('current_game_id');
  }
  return null;
}

export function clearCurrentGameId(): void {
  if (typeof window !== 'undefined') {
    localStorage.removeItem('current_game_id');
    localStorage.removeItem('current_game_timestamp');
  }
}

export function hasCurrentGame(): boolean {
  return getCurrentGameId() !== null;
}
