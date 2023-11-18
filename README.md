# courier_optimisation

Project Description: Courier Optimization - CVRP Solver App using Tabu Search for Multiple Vehicles (ASP.NET MVC)

Courier Optimizer is a GitHub project that offers a comprehensive solution for the Capacitated Vehicle Routing Problem (CVRP) for
multiple vehicles. Leveraging tabu search, this app minimizes total route length while optimizing vehicle utilization.
Built with ASP.NET MVC, it provides a user-friendly interface and a visual path display for intuitive route visualization.

The CVRP Solver App addresses the challenge of efficiently routing a fleet of vehicles with limited capacities,
considering customer locations, demands, and vehicle constraints. By implementing the powerful tabu search algorithm,
Courier Optimizer generates optimized solutions that minimize route length while respecting capacity limitations.
The app applies tabu search to generate optimal solutions, minimizing route length while optimizing vehicle usage.

Courier Optimization's visual path display feature enhances understanding and facilitates decision-making.
It provides interactive visualizations of the optimized routes, enabling logistics managers and transportation professionals
to evaluate efficiency and improve operations.

Input file contains base coordiantes in the first line (its important to set base weight as 0), and all stations coordinates
with their weight.
Example input file:
```
39 1 0
56 28 1
82 57 4
12 41 2
20 66 5
69 33 4
64 6 5
67 14 3
88 38 1
89 79 2
36 95 2
48 29 1
77 18 3
45 78 2
27 11 7
60 54 3
```
