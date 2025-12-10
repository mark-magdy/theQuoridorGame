// import React from 'react';
// import { motion } from 'framer-motion';
// import { Player } from '@/types/gameTypes';
// import { PLAYER_COLOR_CLASSES, PLAYER_TEXT_CLASSES, PLAYER_COLOR_NAMES } from '@/lib/constants';

// interface TurnIndicatorProps {
//   currentPlayer: Player;
//   turnNumber: number;
//   totalMoves: number;
// }

// const TurnIndicator: React.FC<TurnIndicatorProps> = ({
//   currentPlayer,
//   turnNumber,
//   totalMoves,
// }) => {
//   return (
//     <motion.div
//       layout
//       className="bg-white dark:bg-gray-800 rounded-lg shadow-lg p-6 text-center"
//       animate={{
//         boxShadow: [
//           '0 4px 6px rgba(0, 0, 0, 0.1)',
//           '0 10px 20px rgba(59, 130, 246, 0.3)',
//           '0 4px 6px rgba(0, 0, 0, 0.1)',
//         ],
//       }}
//       transition={{
//         duration: 2,
//         repeat: Infinity,
//       }}
//     >
//       <div className="text-sm text-gray-500 dark:text-gray-400 mb-2">
//         Turn {turnNumber} · Move {totalMoves + 1}
//       </div>
      
//       <div className="flex items-center justify-center gap-3">
//         <motion.div
//           className={`
//             w-8 h-8 rounded-full border-4
//             ${PLAYER_COLOR_CLASSES[currentPlayer.color]}
//             flex items-center justify-center text-white font-bold
//           `}
//           animate={{
//             scale: [1, 1.2, 1],
//           }}
//           transition={{
//             duration: 1,
//             repeat: Infinity,
//           }}
//         >
//           {currentPlayer.id + 1}
//         </motion.div>
        
//         <div>
//           <div className={`text-xl font-bold ${PLAYER_TEXT_CLASSES[currentPlayer.color]}`}>
//             {PLAYER_COLOR_NAMES[currentPlayer.color]}&apos;s Turn
//           </div>
//           <div className="text-sm text-gray-600 dark:text-gray-400">
//             {currentPlayer.wallsRemaining} walls left
//           </div>
//         </div>
//       </div>
//     </motion.div>
//   );
// };

// export default TurnIndicator;

