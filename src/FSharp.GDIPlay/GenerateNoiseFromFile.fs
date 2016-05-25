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

        generateNoise.Text <- "Load File"
        generateNoise.Width <- 120
        generateNoise.Height <- 24
        generateNoise.Top <- 25
        generateNoise.Click.Add(fun a ->
            match Utilities.GetFileFromDialog() with
            | Some path ->
                let img =
                    File.ReadAllText(path)
                    |> Tools.deserializeRankedColors
                    |> Tools.generateNoiseFromRankedColors 500 500

                noiseImage.Image <- img
            | _ -> ()
        )

        noiseImage.AutoSize <- true
        noiseImage.Top <- 50

        form.Controls.Add(noiseImage)
        form.Controls.Add(generateNoise)

        form
