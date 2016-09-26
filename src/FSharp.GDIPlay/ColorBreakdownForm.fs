namespace FSharp.GDIPlay

open System
open System.Diagnostics
open System.Drawing
open System.Windows.Forms

module ColorBreakdownForm =
    let GetForm =
        let form = new Form()
        let imageControl = new PictureBox()
        let imageBefore = new PictureBox()
        let imageAfter = new PictureBox()
        let showColorDistribution = new Button()

        showColorDistribution.Text <- "Show Color Dist."
        showColorDistribution.Width <- 120
        showColorDistribution.Height <- 24
        showColorDistribution.Top <- 25
        showColorDistribution.Click.Add(fun a ->
            match Utilities.GetFileFromDialog() with
            | Some path ->
                let img = new Bitmap(path)
                let rawImgData = img |> Tools.getByteArrayForImage

                let colorList = 
                    match (rawImgData.data |> Tools.imageByteArrayToRGBArray) with
                    | Some colors -> colors
                    | None -> []

                let rankedColors = colorList |> Tools.rankColors

                imageControl.Image <- rankedColors |> Tools.graphRankedColors 200

                imageBefore.Image <- img
                imageAfter.Left <- (imageBefore.Width + 10)

                match Utilities.GetFileFromDialog() with
                | Some path -> 
                    let mask = new Bitmap(path)

                    let noise = rankedColors |> Tools.generateNoiseFromRankedColors mask.Width mask.Height
                    let noiseData = noise |> Tools.getByteArrayForImage
                    let noiseColors =
                        match noiseData.data |> Tools.imageByteArrayToRGBArray with
                        | Some colors -> colors
                        | _ -> []

                    let maskColors = 
                        match ((mask |> Tools.getByteArrayForImage).data |> Tools.imageByteArrayToRGBArray)  with
                        | Some colors -> colors
                        | _ -> []

                    let maskedImage =
                        List.map2 (fun (c1:Tools.RGBA) (c2:Tools.RGBA) ->
                            {
                                Tools.RGBA.red = c2.red;
                                Tools.RGBA.green = c2.green;
                                Tools.RGBA.blue = c2.blue;
                                Tools.RGBA.alpha = c1.alpha;
                            }
                        ) maskColors noiseColors 
                        |> Tools.colorListToByteArray
                        |> Tools.getImageData noiseData.stride
                        |> Tools.newBitmapFromImageData

                    imageAfter.Image <- maskedImage
                | None -> ()

                //imageAfter.Image <- noise
            | _ -> ()
        )

        imageControl.Top <- 55
        imageControl.AutoSize <- true
        imageBefore.Top <- 265
        imageBefore.AutoSize <- true
        imageAfter.Top <- 265
        imageAfter.AutoSize <- true
        imageAfter.Click.Add(fun _ ->
            let saveDialog = new SaveFileDialog()
            saveDialog.Filter <- "JPEG File (*.jpg)|*.jpg"
            saveDialog.Title  <- "Save Processed Image"
            match saveDialog.ShowDialog() with
            | DialogResult.OK ->
                let saveFilePath = saveDialog.FileName
                imageAfter.Image.Save(saveFilePath, Imaging.ImageFormat.Jpeg)
            | _ -> () )
        form.Controls.Add(imageControl)
        form.Controls.Add(imageBefore)
        form.Controls.Add(imageAfter)
        form.Controls.Add(showColorDistribution)

        form

