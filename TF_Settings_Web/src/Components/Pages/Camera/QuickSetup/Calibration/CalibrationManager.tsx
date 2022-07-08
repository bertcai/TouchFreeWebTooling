/* eslint-disable @typescript-eslint/no-empty-function */
import 'Styles/Camera/Calibrate.css';

import React, { useEffect } from 'react';
import { Navigate, Route, Routes, useNavigate } from 'react-router-dom';

import { ConfigurationManager } from 'TouchFree/Configuration/ConfigurationManager';
import { InteractionConfig, PhysicalConfig, Vector } from 'TouchFree/Configuration/ConfigurationTypes';
import { ConfigState } from 'TouchFree/Connection/TouchFreeServiceTypes';
import { InteractionType } from 'TouchFree/TouchFreeToolingTypes';

import { PositionType } from 'Components/Pages/Camera/QuickSetup/PositionSelectionScreen';

import CalibrationCompleteScreen from './CalibrationCompleteScreen';
import { CalibrationBottomScreen, CalibrationTopScreen } from './CalibrationScreens';

const calibInteractionConfig: Partial<InteractionConfig> = {
    InteractionType: InteractionType.HOVER,
    DeadzoneRadius: 0.007,
    HoverAndHold: {
        HoverStartTimeS: 1,
        HoverCompleteTimeS: 5,
    },
};

interface CalibrationManager {
    activePosition: PositionType;
}

const CalibrationManager: React.FC<CalibrationManager> = ({ activePosition }) => {
    const [physicalConfig, setPhysicalConfig] = React.useState<PhysicalConfig>();
    const [interactionConfig, setInteractionConfig] = React.useState<InteractionConfig>();

    const navigate = useNavigate();

    useEffect(() => {
        setCursorDisplay(false);
        // Save current config then change it to use config for calibration
        ConfigurationManager.RequestConfigState((config: ConfigState) => {
            setInteractionConfig(config.interaction);
            setPhysicalConfig(config.physical);

            ConfigurationManager.RequestConfigChange(
                calibInteractionConfig,
                { LeapRotationD: getRotationFromPosition(activePosition) },
                () => {}
            );
        });
    }, []);

    const setCalibInteractionConfig = (): void =>
        ConfigurationManager.RequestConfigChange(calibInteractionConfig, {}, () => {});

    const resetCalibConfig = (): void =>
        ConfigurationManager.RequestConfigChange(interactionConfig ?? null, physicalConfig ?? null, () => {
            navigate('/settings/camera/quick/');
        });

    const resetCalibInteractionConfig = (): void =>
        ConfigurationManager.RequestConfigChange(interactionConfig ?? null, {}, () => {});

    return (
        <Routes>
            <Route
                path="top"
                element={
                    <CalibrationTopScreen
                        onCancel={() => {
                            setCursorDisplay(true);
                            resetCalibConfig();
                        }}
                    />
                }
            />
            <Route
                path="bottom"
                element={
                    <CalibrationBottomScreen
                        onCancel={() => {
                            setCursorDisplay(true);
                            resetCalibConfig();
                        }}
                    />
                }
            />
            <Route
                path="complete"
                element={
                    <CalibrationCompleteScreen
                        onLoad={() => {
                            setCursorDisplay(true);
                            resetCalibInteractionConfig();
                        }}
                        onRedo={() => {
                            setCursorDisplay(false);
                            setCalibInteractionConfig();
                        }}
                    />
                }
            />
            <Route path="*" element={<Navigate to="top" replace />} />
        </Routes>
    );
};

export default CalibrationManager;

const setCursorDisplay = (show: boolean) => {
    const svgCanvas = document.querySelector('#svg-cursor') as HTMLElement;
    if (!svgCanvas) return;

    // Add an opacity of 0 to hide the cursor and remove this opacity to show the cursor
    svgCanvas.style.opacity = show ? '' : '0';
};

const getRotationFromPosition = (position: PositionType): Vector => {
    if (position === 'FaceScreen') {
        return { X: 20, Y: 0, Z: 180 };
    }
    if (position === 'FaceUser') {
        return { X: -20, Y: 0, Z: 180 };
    }
    // position === 'Below' (Desktop)
    return { X: 0, Y: 0, Z: 0 };
};
