import {motion } from "framer-motion";
export default function LogoAndTitle() {
    return (<motion.div
                    initial={{ scale: 0.8 }}
                    animate={{ scale: 1 }}
                    transition={{ duration: 0.5, type: 'spring' }}
                    className="text-center mb-12"
                >
                    <h1 className="text-6xl font-bold text-gray-900 dark:text-white mb-4">
                        Quoridor
                    </h1>
                    <p className="text-xl text-gray-600 dark:text-gray-400">
                        Board Game
                    </p>
                    <p className="text-sm text-gray-500 dark:text-gray-400 mt-2">
                        Block your opponents, reach your goal!
                    </p>
                </motion.div>);
}