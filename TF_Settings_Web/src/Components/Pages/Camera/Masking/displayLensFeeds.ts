import { Lens } from './MaskingScreen';

let conversionArraysInitialised = false;
const byteConversionArray = new Uint32Array(256);
const byteConversionArrayOverExposed = new Uint32Array(256);

export const displayLensFeeds = (
    data: ArrayBuffer,
    canvasRef: HTMLCanvasElement,
    lens: Lens,
    isCameraReversed: boolean,
    showOverexposedAreas: boolean
) => {
    const context = getContext(canvasRef);
    if (!context) return;

    if (!conversionArraysInitialised) {
        for (let i = 0; i < 256; i++) {
            byteConversionArray[i] = (255 << 24) | (i << 16) | (i << 8) | i;
            // -13434625 = #FFFF0033 in signed 2's complement
            byteConversionArrayOverExposed[i] = i > 128 ? -13434625 : byteConversionArray[i];
        }
        conversionArraysInitialised = true;
    }

    const conversionArrayToUse = showOverexposedAreas ? byteConversionArrayOverExposed : byteConversionArray;

    const startOfBuffer = new DataView(data, 0, 10);

    const dim1 = startOfBuffer.getUint32(1);
    const dim2 = startOfBuffer.getUint32(5);

    const width = Math.min(dim1, dim2);
    const lensHeight = Math.max(dim1, dim2) / 2;

    const buf = new ArrayBuffer(width * lensHeight * 4);
    const buf8 = new Uint8ClampedArray(buf);
    const buf32 = new Uint32Array(buf);

    const rotated90 = dim2 < dim1;
    const offset = 9;

    if (rotated90) {
        processRotatedScreen(data, lens, buf32, conversionArrayToUse, offset, width, lensHeight);
    } else {
        processScreen(data, lens, buf32, conversionArrayToUse, offset, width, lensHeight);
    }

    // Set black pixels to remove flashing camera bytes
    const startOffset = isCameraReversed ? 0 : (lensHeight - 1) * width;
    buf32.fill(0xff000000, startOffset, startOffset + width);

    canvasRef.width = width;
    canvasRef.height = lensHeight;
    context.putImageData(new ImageData(buf8, width, lensHeight), 0, 0);
};

const processRotatedScreen = (
    data: ArrayBuffer,
    lens: Lens,
    buf32: Uint32Array,
    byteConversionArray: Uint32Array,
    offset: number,
    width: number,
    lensHeight: number
) => {
    let rowBase = 0;
    const offsetView = new Uint8Array(data, offset, width * lensHeight * 2);

    for (let rowIndex = 0; rowIndex < width; rowIndex++) {
        let rowStart = rowBase * 2;
        if (lens === 'Right') {
            for (let i = 0; i < lensHeight; i++) {
                buf32[i + rowBase] = byteConversionArray[offsetView[i + rowStart]];
            }
        }

        rowStart += lensHeight;
        if (lens === 'Left') {
            for (let i = 0; i < lensHeight; i++) {
                buf32[i + rowBase] = byteConversionArray[offsetView[i + rowStart]];
            }
        }

        rowBase += lensHeight;
    }
};

const processScreen = (
    data: ArrayBuffer,
    lens: Lens,
    buf32: Uint32Array,
    byteConversionArray: Uint32Array,
    offset: number,
    width: number,
    lensHeight: number
) => {
    const offsetView = new Uint8Array(data, offset + (lens === 'Left' ? width * lensHeight : 0), width * lensHeight);

    for (let i = 0; i < width * lensHeight; i++) {
        buf32[i] = byteConversionArray[offsetView[i]];
    }
};

const canvasElements: HTMLCanvasElement[] = [];
const canvasContexts: CanvasRenderingContext2D[] = [];

const getContext = (canvasElement: HTMLCanvasElement | null) => {
    if (!canvasElement) {
        return null;
    }
    const canvasIndex = canvasElements.indexOf(canvasElement);
    if (canvasIndex < 0) {
        const context = canvasElement.getContext('2d');
        if (context === null) {
            return context;
        }
        const newIndex = canvasElements.length;
        canvasElements[newIndex] = canvasElement;
        canvasContexts[newIndex] = context;
        return context;
    }
    return canvasContexts[canvasIndex];
};
