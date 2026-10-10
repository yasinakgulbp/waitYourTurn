# Survival train: material and lighting study

**Superseded by user decision D50:** use the previous game train without surface
textures, pending the user's new texture idea. This study was not accepted or
installed. Preserve it as editable source; do not apply it automatically.

This folder is a **visual experiment**, outside Unity's `Assets` directory. The
current game, colliders, navigation, door identities and imported materials have
not been replaced by this study.

The user rejected the previous gray wall / muddy continuous floor revision.
The target remains the supplied `09.44.05` reference: worn warm interior steel,
deep red doors, cool silver edges, a fine dark patterned metal deck, warm accents
against a dark cool environment. A second gray recolor is not the target.

## Editable source

`SurvivalTrain-LookDev.blend` contains original native procedural material nodes
and the packed `OriginalSurfaceAtlas.png`, created with built-in ImageGen.
It loads the exact current game meshes; every texture pattern is evaluated in
metres rather than stretched to fill each mesh's old atlas island. It does not
download third-party textures or project the reference image onto the model.
`surface-atlas-prompt.txt` preserves the exact generation prompt. Warm inner steel,
red paint, charcoal enamel and silver grain are four cells of this original
atlas. The floor pattern, roughness and tiny bump remain authored node networks.
The cells use metric face projection, rather than stretching photographed
manufactured panels over each small mesh part.

`material-lighting-preview.png` is an actual Blender render of that scene.
`material-detail.png` is a separately rendered closer camera in the same scene;
it is not an edited enlargement or a generated game screenshot.
`material-manifest.json` records what is a trial versus a runtime asset.

`*-color.png` are four original 512px unlit 1m-square color swatches baked from
the same materials. They are visual samples, not the final game atlas. Their
panels, repetition edges and downstream Unity UVs still need production baking.

Reproduce from the repository root:

```powershell
& 'C:/Program Files/Blender Foundation/Blender 5.2/blender.exe' --background --python Tools/TrainArt/lookdev_train.py
& 'C:/Program Files/Blender Foundation/Blender 5.2/blender.exe' --background --python Tools/TrainArt/bake_lookdev_swatches.py
& 'C:/Program Files/Blender Foundation/Blender 5.2/blender.exe' --background --python Tools/TrainArt/render_lookdev_detail.py
```

## Review before integration

1. Judge the **whole carriage**, not just a flat albedo texture: floor pattern
   density, warm interior versus red paint, edge highlights and shadow depth.
2. Bench/partition silhouettes are a visual proposal, not installed furniture.
   Their gameplay footprint must be agreed before exporting them. The wide
   longitudinal gangways and door approaches remain the required layout.
3. Native node materials and Cycles lights are offline authoring tools. They are
   **not** shaders or lights that will be copied wholesale into the Android game.
4. After the visual direction is accepted, bake the relevant color, roughness,
   normal and static occlusion detail into shared compact maps. Keep authored
   texel density and reuse. Validate one carriage in the actual Unity renderer
   before converting all the train. A Blender render cannot approve Unity's
   lighting, mobile performance or final advertisement appearance.
5. Mobile acceptance still requires an Android capture and measured frame time,
   memory and temperature; the earlier blockout FPS result is insufficient.

The existing art installation and short gameplay tests remain available; this
study deliberately performs no scene installation and no long gameplay test.
