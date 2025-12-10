import { PlayerColor } from '@/types/gameTypes';

export const BOARD_SIZES = [7, 9, 11] as const;
export const DEFAULT_BOARD_SIZE = 9;
export const PLAYER_COUNTS = [2, 3, 4] as const;
export const DEFAULT_PLAYER_COUNT = 2;

export const PLAYER_COLORS: PlayerColor[] = ['player1', 'player2', 'player3', 'player4'];

export const PLAYER_COLOR_NAMES: Record<PlayerColor, string> = {
  player1: 'Red',
  player2: 'Blue',
  player3: 'Green',
  player4: 'Yellow',
};

export const PLAYER_COLOR_CLASSES: Record<PlayerColor, string> = {
  player1: 'bg-player1',
  player2: 'bg-player2',
  player3: 'bg-player3',
  player4: 'bg-player4',
};

export const PLAYER_BORDER_CLASSES: Record<PlayerColor, string> = {
  player1: 'border-player1',
  player2: 'border-player2',
  player3: 'border-player3',
  player4: 'border-player4',
};

export const PLAYER_TEXT_CLASSES: Record<PlayerColor, string> = {
  player1: 'text-player1',
  player2: 'text-player2',
  player3: 'text-player3',
  player4: 'text-player4',
};

export const WALLS_PER_PLAYER: Record<number, number> = {
  2: 10,
  3: 7,
  4: 5,
};

export const MAX_HISTORY_LENGTH = 50;

export const STORAGE_KEYS = {
  GAME_STATE: 'quoridor_game_state',
  SETTINGS: 'quoridor_settings',
} as const;

// Movement directions
export const DIRECTIONS = {
  UP: { row: -1, col: 0 },
  DOWN: { row: 1, col: 0 },
  LEFT: { row: 0, col: -1 },
  RIGHT: { row: 0, col: 1 },
} as const;

export const DIAGONAL_DIRECTIONS = {
  UP_LEFT: { row: -1, col: -1 },
  UP_RIGHT: { row: -1, col: 1 },
  DOWN_LEFT: { row: 1, col: -1 },
  DOWN_RIGHT: { row: 1, col: 1 },
} as const;


