# Asteroid Collider Test

This is a proof of concept for building pixel perfect colliders for an arbitrary sized asteroid, with the ability to dig through them and dynamically regenerate the colliders.

It uses a basic quadtree to build out a series of box colliders, which are dynamically recalculated, only for affected nodes, when the texture changes.

Dynamic rotation of the game object, which is desired, unfortunately dramatically decreases performance due to the number of colliders.
