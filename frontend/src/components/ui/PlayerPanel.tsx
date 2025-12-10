// import React from 'react';
// import { motion } from 'framer-motion';
// import { Player } from '@/types/gameTypes';
// import { PLAYER_COLOR_CLASSES, PLAYER_BORDER_CLASSES, PLAYER_TEXT_CLASSES, PLAYER_COLOR_NAMES } from '@/lib/constants';

// interface PlayerPanelProps {
//   players: Player[];
//   currentPlayerId: number;
//   winner: Player | null;
// }

// const PlayerPanel: React.FC<PlayerPanelProps> = ({ players, currentPlayerId, winner }) => {
//   return (
//     <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 w-full max-w-4xl">
//       {players.map((player) => {
//         const isCurrentPlayer = player.id === currentPlayerId;
//         const isWinner = winner?.id === player.id;

//         return (
//           <motion.div
//             key={player.id}
//             layout
//             className={`
//               relative p-4 rounded-lg shadow-lg border-4
//               ${PLAYER_BORDER_CLASSES[player.color]}
//               ${isCurrentPlayer
//                 ? 'bg-gradient-to-br from-blue-50 to-blue-100 dark:from-blue-900 dark:to-blue-800'
//                 : 'bg-white dark:bg-gray-800'
//               }
//               ${isWinner ? 'ring-4 ring-yellow-400' : ''}
//             `}
//             animate={{
//               scale: isCurrentPlayer ? 1.05 : 1,
//             }}
//             transition={{ duration: 0.3 }}
//           >
//             {/* Current player indicator */}
//             {isCurrentPlayer && (
//               <motion.div
//                 className="absolute -top-2 -right-2 bg-blue-600 text-white text-xs font-bold px-2 py-1 rounded-full"
//                 animate={{
//                   scale: [1, 1.1, 1],
//                 }}
//                 transition={{
//                   duration: 1,
//                   repeat: Infinity,
//                 }}
//               >
//                 YOUR TURN
//               </motion.div>
//             )}

//             {/* Winner indicator */}
//             {isWinner && (
//               <motion.div
//                 className="absolute -top-2 -right-2 bg-yellow-400 text-gray-900 text-xs font-bold px-2 py-1 rounded-full"
//                 animate={{
//                   rotate: [0, 10, -10, 0],
//                 }}
//                 transition={{
//                   duration: 0.5,
//                   repeat: Infinity,
//                 }}
//               >
//                 🏆 WINNER
//               </motion.div>
//             )}

//             {/* Player info */}
//             <div className="flex items-center gap-3">
//               <div
//                 className={`
//                   w-12 h-12 rounded-full border-4
//                   ${PLAYER_COLOR_CLASSES[player.color]}
//                   ${PLAYER_BORDER_CLASSES[player.color]}
//                   flex items-center justify-center text-white font-bold text-lg
//                   shadow-lg
//                 `}
//               >
//                 {player.id + 1}
//               </div>
//               <div className="flex-1">
//                 <h3 className={`font-bold text-lg ${PLAYER_TEXT_CLASSES[player.color]}`}>
//                   {PLAYER_COLOR_NAMES[player.color]}
//                 </h3>
//                 <p className="text-sm text-gray-600 dark:text-gray-400">
//                   {player.name}
//                 </p>
//               </div>
//             </div>

//             {/* Walls remaining */}
//             <div className="mt-3">
//               <div className="flex justify-between items-center text-sm mb-1">
//                 <span className="text-gray-600 dark:text-gray-400">Walls:</span>
//                 <span className="font-bold text-gray-900 dark:text-white">
//                   {player.wallsRemaining}
//                 </span>
//               </div>
//               <div className="flex gap-1">
//                 {Array.from({ length: 10 }).map((_, i) => (
//                   <div
//                     key={i}
//                     className={`
//                       h-2 flex-1 rounded-full
//                       ${i < player.wallsRemaining
//                         ? PLAYER_COLOR_CLASSES[player.color]
//                         : 'bg-gray-300 dark:bg-gray-600'
//                       }
//                     `}
//                   />
//                 ))}
//               </div>
//             </div>

//             {/* Goal indicator */}
//             <div className="mt-2 text-xs text-gray-500 dark:text-gray-400">
//               Goal: Row {player.goalRow + 1}
//             </div>
//           </motion.div>
//         );
//       })}
//     </div>
//   );
// };

// export default PlayerPanel;



