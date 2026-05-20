[brush]: https://github.com/user-attachments/assets/01db87d0-8223-4729-a627-6e2e8ce51ecd
[bucket-fill]: https://github.com/user-attachments/assets/34f1c19c-f906-404b-abaa-b699de5a761a
[eraser]: https://github.com/user-attachments/assets/abf35f19-0e6d-4e77-aceb-1c408dd41861
[eyedropper]: https://github.com/user-attachments/assets/5d0d405f-a87d-494d-81d7-4916c01f38ba
[intro-paint]: https://github.com/user-attachments/assets/0d27d17b-ac7c-4a4f-979e-b8e4c8d6adf3
[layer-add]: https://github.com/user-attachments/assets/fb6107c6-e312-4240-95c4-bac71cfe6823
[layer-blend]: https://github.com/user-attachments/assets/73bc3c96-ad94-4d78-ba9d-db637e97da65
[layer-hide]: https://github.com/user-attachments/assets/9158bfdf-d893-4cd3-a4e1-eae3222669b2
[layer-remove]: https://github.com/user-attachments/assets/d9de6327-e9bd-439e-a2d4-37b2ccf50a70
[layer-rename]: https://github.com/user-attachments/assets/7e5e41cd-b17f-4d29-84d1-93e2f58bdcf4
[layer-reorder]: https://github.com/user-attachments/assets/90d094fc-0616-489f-b48a-70c4d65c63aa
[layer-select]: https://github.com/user-attachments/assets/5f03cc1f-d975-4377-a74e-e64b127e73ff
[load]: https://github.com/user-attachments/assets/be6f45fd-7c45-4064-8878-3eb5793d653a
[new]: https://github.com/user-attachments/assets/85de2397-faa9-4bd5-bb15-4e5b3e9bf32a
[pan]: https://github.com/user-attachments/assets/cf3e7275-6a29-41eb-b5f2-117fefb418e2
[save]: https://github.com/user-attachments/assets/265a78ed-4db9-49da-8aee-eb24b8ed971e
[select-move]: https://github.com/user-attachments/assets/a51ba870-8171-46ca-8dc6-3e7f17f9f81e
[select-paint]: https://github.com/user-attachments/assets/d9b30c18-7fdc-431b-b732-697205d4eacc
[select-rotate]: https://github.com/user-attachments/assets/3163e692-beb2-4ba3-a245-ccc987b5417f
[select-scale]: https://github.com/user-attachments/assets/c3fe22bc-c113-414f-b779-3d2b61c6c7e8
[undo-redo]: https://github.com/user-attachments/assets/7b43db75-2663-4fa3-af8e-2c925db5a845
[zoom]: https://github.com/user-attachments/assets/629f9972-eeb9-4fe7-92b2-764541508da5

[color-wheel-img]: https://github.com/user-attachments/assets/f966cd27-7985-4ef0-8b9d-1069e28baf5f
[info-img]: https://github.com/user-attachments/assets/12804e74-093e-4bf2-b15a-e8cf7a059713
[layers-img]: https://github.com/user-attachments/assets/c7d0c948-9369-4f65-abbc-92f58e33b33f
[mainmenu-edit-img]: https://github.com/user-attachments/assets/9520b772-b6f7-439e-aabc-82a5726a8531
[mainmenu-file-img]: https://github.com/user-attachments/assets/32d20f7c-3750-4ab7-8b43-a370660a78e2
[paint-editor-img]: https://github.com/user-attachments/assets/b062cc43-1af3-4d0b-97f7-53e63d82df9c
[paint-editor-logo-img]: https://github.com/user-attachments/assets/56fda3fa-93a0-4d70-b70a-c2c2ac146329
[toolbox-img]: https://github.com/user-attachments/assets/8d1ff3f7-19df-4b4c-bd13-885d6143234b
[workspace-img]: https://github.com/user-attachments/assets/2613118d-461d-4800-8dc8-80e1945155f6

<h1 align="center">
  <img width="256" height="256" alt="Paint Editor Logo" src="https://github.com/user-attachments/assets/56fda3fa-93a0-4d70-b70a-c2c2ac146329"/>
  <br>
  Paint Editor Extension for Unity
</h1>

<br>

Paint Editor is an extension for painting and editing textures inside Unity. It provide different tools to paint, select, use layers, and more.

![Paint Editor Extension][paint-editor-img]

# Workspace

| Editor element | Description | Screenshot |
| :------: | ----------- | :----------: |
| Canvas | The area where you paint and edit textures. | ![Canvas][workspace-img] |
| Toolbox | The window where you select the different tools. | ![Toolbox][toolbox-img] |
| Main Menu | The top bar where some functionalities are displayed. The two available tabs are: File and Edit | ![Main Menu Edit][mainmenu-file-img] ![Main Menu Edit][mainmenu-edit-img] |
| Layers | The window where you can add, remove, hide, rename and reorder layers. You can also select blending modes for layers. | ![Layers][layers-img] |
| Color | When clicking on the color field in the Layers' window, the Unity's color wheel pops up. | ![Color][color-wheel-img] |
| Info | Some information related to the canvas is displayed at the bottom of the window. | ![Info][info-img] |

# Main Menu

| Menu function | Description | Video demo |
| :------: | ----------- | :----------: |
| New image | Create a new canvas with any width and height. | ![New image][new] |
| Save image | Save and encode current canvas image into '.png' file at selected path. | ![Save image][save] |
| Load image | Load any image file at selected path. | ![Load image][load] |
| Command History (Undo/Redo) | Use the Edit Menu to restore and redo actions. **Shortcut**: 'ctrl'+'z'/'ctrl'+'shift'+'z' | ![Undo Redo][undo-redo] |

# Tools

| Tool | Description | Video demo |
| :------: | ----------- | :----------: |
| Brush | Paint and adjust its size. There are two brush types: Box and Rect. | ![Brush][brush] |
| Eraser | Erase pixels from the canvas, similar to the brush tool. | ![Eraser][eraser] |
| Bucket | Click in a closed area to paint it. | ![Bucket][bucket-fill] |
| Eyedropper | Use Unity's eyedropper to select any color in the canvas (any color at screen really). | ![Eyedropper][eyedropper] |
| Navigation  | You can use the navigation tool to move the canvas in the window space. **Shortcut**: Hold 'alt' + click. | ![Navigation][pan] |
| Zoom | You can zoom the canvas in or out. **Shortcut**: Hold 'ctrl' + click or use the scroll wheel. | ![Zoom][zoom] |
| Selection | Use the selection tool to take a section from a layer in the canvas and edit it. You can move, rotate and scale up and down the selected area. Also, the selection can be painted, erased and filled inside. | ![Select Move][select-move] ![Select Paint][select-paint] ![Select Rotate][select-rotate] ![Select Scale][select-scale] |

# Layers

| Layer function | Description | Video demo |
| :------: | ----------- | :----------: |
| Add layers | Add new layer. | ![Layer add][layer-add] |
| Select layers | Select layer to paint in. | ![Select layer][layer-select] |
| Reorder layers | Change layers order. | ![Reorder layer][layer-reorder] |
| Rename layers | Change the layer's name in the text field. | ![Rename layers][layer-rename] |
| Hide layers | Click hide button to hide/show the layer. | ![Hide layers][layer-hide] |
| Remove layers | Remove selected layer. | ![Remove layer][layer-remove] |
| Select blending mode | Select one blending mode for the layer. | ![Layer blending modes][layer-blend] |


