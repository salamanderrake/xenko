﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core.Serialization.Assets;

namespace SiliconStudio.Core.Serialization.Contents
{
    /// <summary>
    /// Serializer for high-level "chunk", used by <see cref="ContentManager"/>.
    /// </summary>
    public interface IContentSerializer
    {
        /// <summary>
        /// Gets the type stored on HDD. Usually matches <see cref="ActualType"/>, but sometimes it might be converted to a different format (i.e. a GPU Texture is saved as an Image).
        /// </summary>
        /// <value>
        /// The type stored on HDD.
        /// </value>
        Type SerializationType { get; }

        /// <summary>
        /// Gets the actual runtime type of object being serialized by <see cref="Serialize"/>. It could be different than <see cref="SerializationType"/> if a conversion happened.
        /// </summary>
        /// <value>
        /// The actual type.
        /// </value>
        Type ActualType { get; }

        /// <summary>
        /// Serializes the specified object.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="stream">The stream.</param>
        /// <param name="obj">The object.</param>
        void Serialize(ContentSerializerContext context, SerializationStream stream, object obj);

        /// <summary>
        /// Constructs the specified object. This is useful if there is any cycle in the object graph reference.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        object Construct(ContentSerializerContext context);
    }

    /// <summary>
    /// A <see cref="IContentSerializer"/> with a specific runtime type.
    /// </summary>
    /// <typeparam name="T">Runtime type being serialized. Expected to match <see cref="IContentSerializer.ActualType"/></typeparam>
    public interface IContentSerializer<T> : IContentSerializer
    {
        void Serialize(ContentSerializerContext context, SerializationStream stream, T obj);
    }
}
