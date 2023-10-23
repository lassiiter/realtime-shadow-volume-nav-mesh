# Realtime Shadow Volume NavMesh
Unity implementation of a NavMesh that is updated and exists solely in the shadows of objects  

![Screenshot 2022-03-03 192727](https://user-images.githubusercontent.com/50963416/156681593-e2453a4e-1e71-41d0-8444-71f2aa6b7a60.png)


## Implementation
1. Manually select objects to exist on shadow physics layer  
2. Raycast from the sun to each vertice of the object  
3. Ignore collision with object and store collision position on environment  
4. Remove internal points via https://en.wikipedia.org/wiki/Gift_wrapping_algorithm to create convex hull  
5. Create polygon from x points on the convex hull  

Repeat  

# Result
![unknown (5)](https://user-images.githubusercontent.com/50963416/156680648-4efe3965-21f3-4627-8352-0188e1d73c66.png)
![unknown (4)](https://user-images.githubusercontent.com/50963416/156680537-c3dfa8c9-8f4f-45bf-9ebe-60a1f7ed53ca.png)
![unknown (3)](https://user-images.githubusercontent.com/50963416/156680492-d88948fc-0099-49f0-b184-9ac1e9c6503a.png)
![unknown (2)](https://user-images.githubusercontent.com/50963416/156680779-b00f538b-2c78-44d8-8cb0-9b1c0842d64e.png)
