namespace FSharp.GDIPlay

open System
open System.Diagnostics
open System.Drawing
open System.IO
open System.Windows.Forms

module GenerateNoiseFromFile =
    let GetForm =
        let form = new Form()
        let generateNoise = new Button()
        let noiseImage = new PictureBox()
        let hexTextBox = new TextBox()
        let mutable dataFilePath = ""
        let reGenerateNoise = new Button()

        hexTextBox.Top <- 50
        hexTextBox.Height <- 25

        reGenerateNoise.Text <- "Re-Generate"
        reGenerateNoise.Width <- 120
        reGenerateNoise.Height <-24
        reGenerateNoise.Top <- 25
        reGenerateNoise.Click.Add(fun a ->
            let color = ColorTranslator.FromHtml(hexTextBox.Text)
            let img =
                File.ReadAllText(dataFilePath)
                |> Tools.deserializeRankedColors
                |> Tools.migrateColors color
                |> Tools.generateNoiseFromRankedColors 985 657

            //let newImg = Tools.colorTransform img color

            noiseImage.Image <- img
        )

        generateNoise.Text <- "Load File"
        generateNoise.Width <- 120
        generateNoise.Height <- 24
        generateNoise.Top <- 0
        generateNoise.Click.Add(fun a ->
            match Utilities.GetFileFromDialog() with
            | Some path ->
                dataFilePath <- path
            | _ -> ()
        )

        noiseImage.AutoSize <- true
        noiseImage.Top <- 125
        noiseImage.Click.Add(fun _ ->
            let saveDialog = new SaveFileDialog()
            saveDialog.Filter <- "PNG File (*.png)|*.png"
            saveDialog.Title  <- "Save Processed Image"
            match saveDialog.ShowDialog() with
            | DialogResult.OK ->
                let saveFilePath = saveDialog.FileName
                noiseImage.Image.Save(saveFilePath, Imaging.ImageFormat.Png)
            | _ -> () )

        form.Controls.Add(noiseImage)
        form.Controls.Add(generateNoise)
        form.Controls.Add(hexTextBox)
        form.Controls.Add(reGenerateNoise)
        form
