﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Linq;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Rendering.Data;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Data;

namespace SiliconStudio.Xenko.Extensions
{
    public static class BoundingExtensions
    {
        public unsafe static BoundingBox ComputeBounds(this VertexBufferBinding vertexBufferBinding, ref Matrix matrix, out BoundingSphere boundingSphere)
        {
            var positionOffset = vertexBufferBinding.Declaration
                .EnumerateWithOffsets()
                .First(x => x.VertexElement.SemanticAsText == "POSITION")
                .Offset;

            var boundingBox = BoundingBox.Empty;
            boundingSphere = new BoundingSphere();

            var vertexStride = vertexBufferBinding.Declaration.VertexStride;
            fixed (byte* bufferStart = &vertexBufferBinding.Buffer.GetSerializationData().Content[vertexBufferBinding.Offset])
            {
                // Calculates bounding box and bounding sphere center
                byte* buffer = bufferStart + positionOffset;
                for (int i = 0; i < vertexBufferBinding.Count; ++i)
                {
                    var position = (Vector3*)buffer;
                    Vector3 transformedPosition;

                    Vector3.TransformCoordinate(ref *position, ref matrix, out transformedPosition);

                    // Prepass calculate the center of the sphere
                    Vector3.Add(ref transformedPosition, ref boundingSphere.Center, out boundingSphere.Center);
                    
                    BoundingBox.Merge(ref boundingBox, ref transformedPosition, out boundingBox);
                    
                    buffer += vertexStride;
                }

                //This is the center of our sphere.
                boundingSphere.Center /= (float)vertexBufferBinding.Count;

                // Calculates bounding sphere center
                buffer = bufferStart + positionOffset;
                for (int i = 0; i < vertexBufferBinding.Count; ++i)
                {
                    var position = (Vector3*)buffer;
                    Vector3 transformedPosition;

                    Vector3.TransformCoordinate(ref *position, ref matrix, out transformedPosition);


                    //We are doing a relative distance comparasin to find the maximum distance
                    //from the center of our sphere.
                    float distance;
                    Vector3.DistanceSquared(ref boundingSphere.Center, ref transformedPosition, out distance);

                    if (distance > boundingSphere.Radius)
                        boundingSphere.Radius = distance;

                    buffer += vertexStride;
                }

                //Find the real distance from the DistanceSquared.
                boundingSphere.Radius = (float)Math.Sqrt(boundingSphere.Radius);
            }

            return boundingBox;
        }
    }
}