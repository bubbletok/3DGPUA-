# 3D GPU A* Pathfinding

![image](https://github.com/user-attachments/assets/84f269b0-4f20-48d3-b175-b01ea5bd00b0)


### Details

- Implemented A* in 3D during Internship
- Using Unity Compute Shader for GPU computation
- Write Compute Shader to pre-compute A* grid and properties
    - Init A* grid and update reachability of each cell by using RayCast, Physics collision
    - Calculate heuristic value for neighbor cell of each cell from current cell to target position.
