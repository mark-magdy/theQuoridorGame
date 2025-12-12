export interface RoomDto {
  roomId: string;
  hostUserId: string;
  maxPlayers: number;
  currentPlayers: number;
  status: string;
  players: RoomPlayerDto[];
}

export interface RoomPlayerDto {
  userId: string;
  username: string;
  isReady: boolean;
  isHost: boolean;
}

export interface CreateRoomDto {
  maxPlayers: number;
}

export interface JoinRoomDto {
  roomId: string;
}

export interface GameMoveDto {
  roomId: string;
  moveType: "move" | "wall";
  toRow: number;
  toCol: number;
  wallOrientation?: "horizontal" | "vertical";
}

export interface ChatMessage {
  userId: string;
  username: string;
  message: string;
  timestamp: Date;
}
