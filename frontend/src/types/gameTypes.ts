export type PlayerColor = 'player1' | 'player2' | 'player3' | 'player4';
export type PlayerType = 'Human' | 'Bot';

export interface Position {
  row: number;
  col: number;
}

export interface Player {
  id: number;
  color: PlayerColor;
  position: Position;
  wallsRemaining: number;
  goalRow: number; // Target row to reach
  name: string;
  userId?: string;
  type?: PlayerType;
  botDifficulty?: number;
}

export interface Wall {
  position: Position; // Top-left corner of the wall
  isHorizontal: boolean;
}

export type MoveType = 'pawn' | 'wall';

export interface Move {
  type: MoveType;
  playerId: number;
  timestamp: number;
  // For pawn moves
  from?: Position;
  to?: Position;
  // For wall placement
  wall?: Wall;
}

export interface GameState {
  boardSize: number;
  players: Player[];
  currentPlayerIndex: number;
  walls: Wall[];
  gameStatus: 'Menu' | 'Waiting' | 'Playing' | 'Paused' | 'Finished';
  winner: number | null;
  moveHistory: Move[];
  historyIndex: number; // For undo/redo
}

export interface GameSettings {
  playerCount: 2 | 3 | 4;
  boardSize: 7 | 9 | 11;
  theme: 'light' | 'dark';
  showValidPaths: boolean;
}

export interface ValidMoves {
  positions: Position[];
  walls: Wall[];
}


