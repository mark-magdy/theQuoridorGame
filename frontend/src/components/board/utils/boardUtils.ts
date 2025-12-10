import { Position, Wall, Player } from '@/types/gameTypes';
import { DIRECTIONS } from '@/lib/constants';

export function positionsEqual(pos1: Position, pos2: Position): boolean {
  return pos1.row === pos2.row && pos1.col === pos2.col;
}

export function isValidPosition(pos: Position, boardSize: number): boolean {
  return pos.row >= 0 && pos.row < boardSize && pos.col >= 0 && pos.col < boardSize;
}

export function getPlayerAtPosition(players: Player[], position: Position): Player | null {
  return players.find(p => positionsEqual(p.position, position)) || null;
}
export function getValidPawnMoves(
  player: Player,
  players: Player[],
  walls: Wall[],
  boardSize: number
): Position[] {
  const validMoves: Position[] = [];
  const currentPos = player.position;

  for (const direction of Object.values(DIRECTIONS)) {
    const adjacentPos: Position = {
      row: currentPos.row + direction.row,
      col: currentPos.col + direction.col,
    };
    

    if (!isValidPosition(adjacentPos, boardSize)) {
      continue;
    }

    if (isWallBlocking(currentPos, adjacentPos, walls)) {
      continue;
    }

    const playerAtAdjacent = getPlayerAtPosition(players, adjacentPos);

    if (!playerAtAdjacent) {
      validMoves.push(adjacentPos);
    } else {
      const jumpMoves = getJumpMoves(
        currentPos,
        adjacentPos,
        playerAtAdjacent.position,
        players,
        walls,
        boardSize
      );
      validMoves.push(...jumpMoves);
    }
  }

  return validMoves;
}

export function canPlaceWall(
  wall: Wall,
  existingWalls: Wall[],
  boardSize: number
): boolean {
  const { position, isHorizontal } = wall;

  // Check if wall is within bounds
  if (isHorizontal) {
    if (position.row <= 0 || position.row > boardSize || 
        position.col < 0 || position.col >= boardSize - 1) {
      return false;
    }
  } else {
    if (position.row < 0 || position.row >= boardSize - 1 || 
        position.col <= 0 || position.col > boardSize) {
      return false;
    }
  }

  // Check for overlapping walls
  for (const existing of existingWalls) {
    if (existing.isHorizontal === isHorizontal) {
      // Same orientation
      if (positionsEqual(existing.position, position)) {
        return false; // Exact overlap
      }
      
      if (isHorizontal) {
        // Check if walls are on same row and adjacent/overlapping
        if (existing.position.row === position.row) {
          const existingCols = [existing.position.col, existing.position.col + 1];
          const newCols = [position.col, position.col + 1];
          if (existingCols.some(c => newCols.includes(c))) {
            return false;
          }
        }
      } else {
        // Check if walls are on same column and adjacent/overlapping
        if (existing.position.col === position.col) {
          const existingRows = [existing.position.row, existing.position.row + 1];
          const newRows = [position.row, position.row + 1];
          if (existingRows.some(r => newRows.includes(r))) {
            return false;
          }
        }
      }
    } else {
      // Different orientation - check for crossing
      if (isHorizontal) {
        if (position.col + 1 === existing.position.col && position.row === existing.position.row + 1) {
          return false;
        }
      } else {
        if (position.row + 1 === existing.position.row  && position.col === existing.position.col +1 ) {
          return false;
        }
      }
    }
  }

  return true;
}

function isWallBlocking(
  from: Position,
  to: Position,
  walls: Wall[]
): boolean {
  const rowDiff = to.row - from.row;
  const colDiff = to.col - from.col;

  // Check if moving vertically
  if (rowDiff !== 0 && colDiff === 0) {
    const minRow = Math.min(from.row, to.row);
    const col = from.col;
    
    // Check for horizontal walls between the positions
    return walls.some(wall => {
      if (wall.isHorizontal) {
        return wall.position.row === minRow+1 && 
               (wall.position.col === col || wall.position.col === col - 1);
      }
      return false;
    });
  }
  
  // Check if moving horizontally
  if (colDiff !== 0 && rowDiff === 0) {
    const row = from.row;
    const minCol = Math.min(from.col, to.col);
    
    // Check for vertical walls between the positions
    return walls.some(wall => {
      if (!wall.isHorizontal) {
        return wall.position.col === minCol+1 && 
               (wall.position.row === row || wall.position.row === row - 1);
      }
      return false;
    });
  }
  
  return false;
}


function getJumpMoves(
  currentPos: Position,
  adjacentPos: Position,
  opponentPos: Position,
  players: Player[],
  walls: Wall[],
  boardSize: number
): Position[] {
  const validJumps: Position[] = [];
  const rowDiff = adjacentPos.row - currentPos.row;
  const colDiff = adjacentPos.col - currentPos.col;

  // Try to jump straight over
  const straightJump: Position = {
    row: opponentPos.row + rowDiff,
    col: opponentPos.col + colDiff,
  };

  if (
    isValidPosition(straightJump, boardSize) &&
    !isWallBlocking(opponentPos, straightJump, walls) &&
    !getPlayerAtPosition(players, straightJump)
  ) {
    validJumps.push(straightJump);
  } else {
    // Can't jump straight, try diagonal moves
    if (rowDiff !== 0) {
      // Moving vertically, try left and right
      const leftDiag: Position = { row: opponentPos.row, col: opponentPos.col - 1 };
      const rightDiag: Position = { row: opponentPos.row, col: opponentPos.col + 1 };

      if (
        isValidPosition(leftDiag, boardSize) &&
        !isWallBlocking(opponentPos, leftDiag, walls) &&
        !getPlayerAtPosition(players, leftDiag)
      ) {
        validJumps.push(leftDiag);
      }

      if (
        isValidPosition(rightDiag, boardSize) &&
        !isWallBlocking(opponentPos, rightDiag, walls) &&
        !getPlayerAtPosition(players, rightDiag)
      ) {
        validJumps.push(rightDiag);
      }
    } else if (colDiff !== 0) {
      // Moving horizontally, try up and down
      const upDiag: Position = { row: opponentPos.row - 1, col: opponentPos.col };
      const downDiag: Position = { row: opponentPos.row + 1, col: opponentPos.col };

      if (
        isValidPosition(upDiag, boardSize) &&
        !isWallBlocking(opponentPos, upDiag, walls) &&
        !getPlayerAtPosition(players, upDiag)
      ) {
        validJumps.push(upDiag);
      }

      if (
        isValidPosition(downDiag, boardSize) &&
        !isWallBlocking(opponentPos, downDiag, walls) &&
        !getPlayerAtPosition(players, downDiag)
      ) {
        validJumps.push(downDiag);
      }
    }
  }

  return validJumps;
}



