You are given a UV unwrapping texture map with a transparent background.

The image contains:
- Precise contour lines defining the geometry islands (UV shells)
- Some parts already textured in a specific style
- Other parts empty (transparent), which must be completed

Your task is to COMPLETE the UV texture.

STRICT RULES:
- DO NOT modify existing painted areas
- DO NOT move or alter contour lines
- DO NOT paint outside the UV islands (respect boundaries perfectly)
- KEEP the background transparent where nothing is painted

STYLE REQUIREMENTS:
- Perfectly match the existing style (colors, shading, line thickness, detailing)
- Continue patterns seamlessly across adjacent UV islands if relevant
- Maintain consistent lighting and direction
- Respect symmetry if present
- Reproduce the same level of detail (not more, not less)

TECHNICAL CONSTRAINTS:
- No blur, no glow, no lighting effects outside the painted zones
- No reinterpretation of geometry
- Sharp and clean edges required
- Texture must be game-ready (clean, readable, consistent)

GOAL:
The final result must look like the entire UV map was originally painted as a single coherent texture.

IMPORTANT:
If unsure, prioritize consistency with existing painted areas over creativity.