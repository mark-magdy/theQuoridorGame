// import React from 'react';
// import Button from '@/components/common/Button';
// import Icon from '@/components/common/Icon';
// import Tooltip from '@/components/common/Tooltip';

// interface GameControlsProps {
//   canUndo: boolean;
//   canRedo: boolean;
//   onUndo: () => void;
//   onRedo: () => void;
//   onSave: () => void;
//   onLoad: () => void;
//   onRestart: () => void;
//   onSettings: () => void;
//   onBackToMenu: () => void;
// }

// const GameControls: React.FC<GameControlsProps> = ({
//   canUndo,
//   canRedo,
//   onUndo,
//   onRedo,
//   onSave,
//   onLoad,
//   onRestart,
//   onSettings,
//   onBackToMenu,
// }) => {
//   return (
//     <div className="flex flex-wrap gap-1.5 sm:gap-2 justify-center items-center p-2 sm:p-4 bg-white dark:bg-gray-800 rounded-lg shadow-lg">
//       <Tooltip content="Undo (Ctrl+Z)">
//         <Button
//           variant="secondary"
//           size="sm"
//           onClick={onUndo}
//           disabled={!canUndo}
//           className="flex items-center gap-1 sm:gap-2 text-xs sm:text-sm px-2 sm:px-3"
//         >
//           <Icon name="undo" size={16} />
//           <span className="hidden sm:inline">Undo</span>
//         </Button>
//       </Tooltip>

//       <Tooltip content="Redo (Ctrl+Shift+Z)">
//         <Button
//           variant="secondary"
//           size="sm"
//           onClick={onRedo}
//           disabled={!canRedo}
//           className="flex items-center gap-1 sm:gap-2 text-xs sm:text-sm px-2 sm:px-3"
//         >
//           <Icon name="redo" size={16} />
//           <span className="hidden sm:inline">Redo</span>
//         </Button>
//       </Tooltip>

//       {/* <div className="hidden sm:block w-px h-8 bg-gray-300 dark:bg-gray-600 mx-2" /> */}

//       {/* <Tooltip content="Save game to localStorage">
//         <Button
//           variant="secondary"
//           size="sm"
//           onClick={onSave}
//           className="flex items-center gap-1 sm:gap-2 text-xs sm:text-sm px-2 sm:px-3"
//         >
//           <Icon name="save" size={16} />
//           <span className="hidden sm:inline">Save</span>
//         </Button>
//       </Tooltip>

//       <Tooltip content="Load saved game">
//         <Button
//           variant="secondary"
//           size="sm"
//           onClick={onLoad}
//           className="flex items-center gap-1 sm:gap-2 text-xs sm:text-sm px-2 sm:px-3"
//         >
//           <Icon name="load" size={16} />
//           <span className="hidden sm:inline">Load</span>
//         </Button>
//       </Tooltip> */}

//       <div className="hidden sm:block w-px h-8 bg-gray-300 dark:bg-gray-600 mx-2" />

//       <Tooltip content="Restart game">
//         <Button
//           variant="secondary"
//           size="sm"
//           onClick={onRestart}
//           className="flex items-center gap-1 sm:gap-2 text-xs sm:text-sm px-2 sm:px-3"
//         >
//           <Icon name="restart" size={16} />
//           <span className="hidden sm:inline">Restart</span>
//         </Button>
//       </Tooltip>

//       <Tooltip content="Settings">
//         <Button
//           variant="secondary"
//           size="sm"
//           onClick={onSettings}
//           className="flex items-center gap-1 sm:gap-2 text-xs sm:text-sm px-2 sm:px-3"
//         >
//           <Icon name="settings" size={16} />
//           <span className="hidden sm:inline">Settings</span>
//         </Button>
//       </Tooltip>

//       <div className="hidden sm:block w-px h-8 bg-gray-300 dark:bg-gray-600 mx-2" />

//       <Button
//         variant="secondary"
//         size="sm"
//         onClick={onBackToMenu}
//         className="flex items-center gap-1 sm:gap-2 text-xs sm:text-sm px-2 sm:px-3"
//       >
//         <Icon name="menu" size={16} />
//         <span className="hidden sm:inline">Home</span>
//       </Button>
//     </div>
//   );
// };

// export default GameControls;



