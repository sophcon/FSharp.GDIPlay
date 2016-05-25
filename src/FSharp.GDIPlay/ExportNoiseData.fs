namespace FSharp.GDIPlay

open System
open System.Diagnostics
open System.Drawing
open System.Windows.Forms

module ExportNoiseData =
    let GetForm =
        let form = new Form()
        let exportNoise = new Button()
        let loadImage = new Button()
        let displayData = new TextBox()

        loadImage.Text <- "Load Image"
        loadImage.Width <- 120
        loadImage.Height <- 24
        loadImage.Top <- 25
        loadImage.Click.Add(fun a ->
            match Utilities.GetFileFromDialog() with
            | Some path ->
                use img = new Bitmap(path)
                let rawImgData = img |> Tools.getByteArrayForImage

                let colorList = 
                    match (rawImgData.data |> Tools.imageByteArrayToRGBArray) with
                    | Some colors -> colors
                    | None -> []

                let colorString = 
                    colorList 
                    |> Tools.rankColors 
                    |> List.map Tools.serializeRankedColor
                    |> List.fold (fun acc colorString -> sprintf "%s;%s" acc colorString) ""

                displayData.Text <- colorString.Substring(1)
            | _ -> ()
        )

        exportNoise.Text <- "Export Noise"
        exportNoise.Width <- 120
        exportNoise.Height <- 24
        exportNoise.Top <- 50
        exportNoise.Click.Add(fun _ ->
            let saveDialog = new SaveFileDialog()
            saveDialog.Filter <- "Text file (*.txt)|*.txt"
            saveDialog.Title  <- "Save Noise Data"
            match saveDialog.ShowDialog() with
            | DialogResult.OK ->
                let saveFilePath = saveDialog.FileName
                use stream = System.IO.File.CreateText(saveFilePath)
                stream.Write(displayData.Text)
                stream.Close()
            | _ -> () )

        displayData.WordWrap <- true
        displayData.Multiline <- true
        displayData.Height <- 300
        displayData.Width <- 800
        displayData.Top <- 100

        form.Controls.Add(displayData)
        form.Controls.Add(loadImage)
        form.Controls.Add(exportNoise)

        form

