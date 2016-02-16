﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Rendering.Images
{
    /// <summary>
    /// The tonemap operator by Mike Day, Insomniac Games.
    /// </summary>
    /// <remarks>
    /// https://d3cw3dd2w32x2b.cloudfront.net/wp-content/uploads/2012/09/an-efficient-and-user-friendly-tone-mapping-operator.pdf
    /// </remarks>
    [DataContract("ToneMapMikeDayOperator")]
    [Display("Mike-Day")]
    public class ToneMapMikeDayOperator : ToneMapOperator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToneMapMikeDayOperator"/> class.
        /// </summary>
        public ToneMapMikeDayOperator()
            : base("ToneMapMikeDayOperatorShader")
        {
            BlackPoint = 0.005f;
            CrossOver = 0.8f;
            WhitePoint = 4.0f;
            Toe = 0.2f;
            Shoulder = 0.8f;
        }

        /// <summary>
        /// Gets or sets the black point.
        /// </summary>
        /// <value>The black point.</value>
        [DataMember(10)]
        [DefaultValue(0.005f)]
        public float BlackPoint { get; set; }

        /// <summary>
        /// Gets or sets the cross over.
        /// </summary>
        /// <value>The cross over.</value>
        [DataMember(20)]
        [DefaultValue(0.8f)]
        public float CrossOver { get; set; }

        /// <summary>
        /// Gets or sets the white point.
        /// </summary>
        /// <value>The white point.</value>
        [DataMember(30)]
        [DefaultValue(4.0f)]
        public float WhitePoint { get; set; }

        /// <summary>
        /// Gets or sets the toe.
        /// </summary>
        /// <value>The toe.</value>
        [DataMember(40)]
        [DefaultValue(0.2f)]
        public float Toe { get; set; }

        /// <summary>
        /// Gets or sets the shoulder.
        /// </summary>
        /// <value>The shoulder.</value>
        [DataMember(50)]
        [DefaultValue(0.8f)]
        public float Shoulder { get; set; }

        public override void UpdateParameters(ColorTransformContext context)
        {
            // TODO This could be put as part 

            double b = BlackPoint;
            double c = CrossOver;
            double w = WhitePoint;
            double t = Toe;
            double s = Shoulder;

            double k = ((1 - t) * (c - b)) / ((1 - s) * (w - c) + (1 - t) * (c - b));

            var toe = new Vector4(
                (float)((k * (1 - t))),
                (float)(-t),
                (float)(k * (1 - t) * (-b)),
                (float)(c - (1 - t) * b));

            var shoulder = new Vector4(
                (float)(((1 - k) + k * s)),
                (float)(s),
                (float)((1 - k) * (-c) + k * ((1 - s) * w - c)),
                (float)((1 - s) * w - c));

            // Don't call base, as we are rewriting all parameters for the shader
            Parameters.SetValueSlow(ToneMapMikeDayOperatorShaderKeys.ToeCoeffs, toe);
            Parameters.SetValueSlow(ToneMapMikeDayOperatorShaderKeys.ShoulderCoeffs, shoulder);
            Parameters.SetValueSlow(ToneMapMikeDayOperatorShaderKeys.MiddleCrossOver, CrossOver);
        }
    }
}