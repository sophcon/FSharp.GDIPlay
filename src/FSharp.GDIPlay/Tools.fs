namespace FSharp.GDIPlay

open System
open System.Drawing
open System.Drawing.Imaging
open System.Runtime.InteropServices

module Tools =
    type RawImageData = {
        data:byte[];
        stride:int
    }

    type RGBA = {
        red:int;
        green:int;
        blue:int;
        alpha:int
    }

    let adjustLevel (channelByte:float32, level:float32) =
        channelByte * level

    let setInRGBRange channelByte =
        Math.Min(Math.Max(channelByte, 255.0f), 0.0f)

    let multiplyChannelByte (sourceChannel:byte) (overlayChannel:byte) =
        (float32(sourceChannel)/255.0f * float32(overlayChannel)/255.0f) * 255.0f |> byte

    let pixelMap2 (sourceImageData:byte[]) (overlayImageData:byte[]) (blendFunction) = 
        Array.map2 blendFunction sourceImageData overlayImageData

    let createSolidColorImage width height color  = 
        let image = new Bitmap(width, height, PixelFormat.Format32bppArgb)
        let graphics = Graphics.FromImage(image)

        graphics.FillRectangle(new SolidBrush(color), 0, 0, width, height)
        image

    let createSolidColorOverlay (image:Bitmap) (color:Color) = 
        createSolidColorImage image.Width image.Height color

    let newImageByteArray (imageData:BitmapData) =
        Array.zeroCreate<byte>(imageData.Stride * imageData.Height)

    let getByteArrayForImage (image:Bitmap) =
        let rect = Rectangle(0, 0, image.Width, image.Height)
        let data = image.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb)
        let buffer = newImageByteArray data

        Marshal.Copy(data.Scan0, buffer, 0, buffer.Length)

        image.UnlockBits data

        { data = buffer; stride = data.Stride }
        
    let newBitmapFromImageData data =
        let height = data.data.Length / data.stride
        let width = data.stride / 4
        let bitmap = new Bitmap(width, height)
        let newImageData = bitmap.LockBits(Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb)

        Marshal.Copy(data.data, 0, newImageData.Scan0, data.data.Length) |> ignore

        bitmap.UnlockBits newImageData |> ignore

        bitmap

    let getImageData stride bytes =
        { data = bytes; stride = stride }

    let blendImages (baseImage:Bitmap) (overlayImage:Bitmap) =
        let baseData = getByteArrayForImage baseImage
        let overlayData = getByteArrayForImage overlayImage
    
        multiplyChannelByte
        |> pixelMap2 baseData.data overlayData.data
        |> getImageData baseData.stride
        |> newBitmapFromImageData
        
    let colorTransform image color =
        createSolidColorOverlay image color
        |> blendImages image

    let testRed (filepath:string) =
        let img = new Bitmap(filepath)

        colorTransform img Color.Red

    let imageByteArrayToRGBArray (bytes:byte[]) =
        let bytesPerColor = 4
        let colorCount = bytes.Length / bytesPerColor
        if bytes.Length % bytesPerColor = 0 then
            [0..colorCount - 1]
            |> List.map (fun colorIndex ->
                { red   = bytes.[colorIndex * bytesPerColor + 0] |> int;
                  green = bytes.[colorIndex * bytesPerColor + 1] |> int;
                  blue  = bytes.[colorIndex * bytesPerColor + 2] |> int;
                  alpha = bytes.[colorIndex * bytesPerColor + 3] |> int})
            |> Some
        else
            None