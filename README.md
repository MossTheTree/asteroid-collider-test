# Asteroid Collider Test

This is a proof of concept for building pixel perfect colliders for an arbitrary sized asteroid, with the ability to dig through them and dynamically regenerate the colliders.

The initial version used a basic quadtree to build out a series of box colliders, which are dynamically recalculated, only for affected nodes, when the texture changes. Updated version uses a CustomCollider2D and a PhysicsShapeGroup2D to dramatically improve performance. Terrain destruction works smoothly on asteroids up to about 256x256 px but slows dramatically on larger asteroids.

Dynamic rotation of the game object, which is desired, works if there is no rigidbody added to the asteroid.
