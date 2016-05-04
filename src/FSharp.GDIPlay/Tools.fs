namespace Sophcon.Builder.Imaging

open System.Drawing
open System.Drawing.Imaging
open System.Runtime.InteropServices

module Tools =
    type RawImageData = {
        data:byte[];
        height:int
    }

    let adjustLevel (channelByte:float32, level:float32) =
        channelByte * level

    let setInRGBRange channelByte =
        System.Math.Min(System.Math.Max(channelByte, 255.0f), 0.0f)

    let multiplyChannelByte (sourceChannel:byte) (overlayChannel:byte) =
        (float32(sourceChannel)/255.0f * float32(overlayChannel)/255.0f) * 255.0f |> byte

    let pixelMap (imageData:byte[]) (adjustFunction) = 
        for index = 0 to imageData.Length - 1 do
            let channelByte = adjustFunction imageData.[index] |> byte
            imageData.SetValue(channelByte, index)

    let pixelMap2 (sourceImageData:byte[]) (overlayImageData:byte[]) (blendFunction) = 
        for index = 0 to sourceImageData.Length - 1 do
            let channelByte = blendFunction sourceImageData.[index] overlayImageData.[index] |> byte
            sourceImageData.SetValue(channelByte, index)

    let createSolidColorImage width height color  = 
        let image = new Bitmap(width, height, PixelFormat.Format32bppArgb)
        let graphics = Graphics.FromImage(image)

        graphics.FillRectangle(new SolidBrush(color), 0, 0, width, height)
        image

    let createSolidColorOverlay (image:Bitmap) (color:Color) = 
        createSolidColorImage image.Width image.Height color

    let newImageByteArray (imageData:BitmapData) =
        Array.zeroCreate<byte>(imageData.Stride * imageData.Height)

    let getByteArrayForImage (image:Bitmap) rect =
        let data = image.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb)
        let buffer = newImageByteArray data

        Marshal.Copy(data.Scan0, buffer, 0, buffer.Length)

        image.UnlockBits data

        { data = buffer; height= data.Stride }

    let blendImages (baseImage:Bitmap) (overlayImage:Bitmap) =
        let baseImageData = baseImage.LockBits(new Rectangle(0, 0, baseImage.Width, baseImage.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb)
        let baseImageBuffer = Array.zeroCreate<byte>(baseImageData.Stride * baseImageData.Height)

        Marshal.Copy(baseImageData.Scan0, baseImageBuffer, 0, baseImageBuffer.Length)

        let overlayImageData = overlayImage.LockBits(new Rectangle(0, 0, baseImage.Width, baseImage.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb)
        let overlayImageBuffer = Array.zeroCreate<byte>(baseImageData.Stride * baseImageData.Height)

        Marshal.Copy(overlayImageData.Scan0, overlayImageBuffer, 0, overlayImageBuffer.Length)
    
        // need to support per channel tweaking
        pixelMap2 baseImageBuffer overlayImageBuffer multiplyChannelByte    

        let resultImage = new Bitmap(baseImage.Width, baseImage.Height, PixelFormat.Format32bppArgb)
        let resultImageData = resultImage.LockBits(new Rectangle(0, 0, resultImage.Width, resultImage.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb)

        Marshal.Copy(baseImageBuffer, 0, resultImageData.Scan0, baseImageBuffer.Length)

        resultImage.UnlockBits resultImageData
        baseImage.UnlockBits baseImageData
        overlayImage.UnlockBits overlayImageData

        resultImage
        
    let colorTransform image color =
        createSolidColorOverlay image color
        |> blendImages image