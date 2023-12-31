// SPDX-License-Identifier: MPL-2.0
namespace Emik.SourceGenerators.Choices;

/// <summary>Generates the attribute needed to use this analyzer.</summary>
[Generator]
public sealed class AttributeGenerator() : FixedGenerator(
    "Emik.ChoiceAttribute",
    $$"""
      namespace Emik
      {
          /// <summary>
          /// Indicates to Emik.SourceGenerators.Choices to extend the type, transforming it to a disjoint union.
          /// </summary>
          [global::System.AttributeUsage(global::System.AttributeTargets.Class | global::System.AttributeTargets.Struct)]
          {{Annotation}}
          internal class ChoiceAttribute : global::System.Attribute
          {
              /// <summary>
              /// Initializes a new instance of the <see cref="ChoiceAttribute"/> class.
              /// </summary>
              internal ChoiceAttribute()
              {
                  Set = null;
                  IsPubliclyMutable = null;
              }
      
              /// <summary>
              /// Initializes a new instance of the <see cref="ChoiceAttribute"/> class.
              /// </summary>
              /// <param name="set">The type of fields that define the annotated union.</param>
              internal ChoiceAttribute(Type set)
              {
                  Set = set;
                  IsPubliclyMutable = null;
              }
      
              /// <summary>
              /// Initializes a new instance of the <see cref="ChoiceAttribute"/> class.
              /// </summary>
              /// <param name="isPubliclyMutable">Determines if mutability is private or public.</param>
              internal ChoiceAttribute(bool isPubliclyMutable)
              {
                  Set = null;
                  IsPubliclyMutable = isPubliclyMutable;
              }
      
              /// <summary>
              /// Initializes a new instance of the <see cref="ChoiceAttribute"/> class.
              /// </summary>
              /// <param name="set">The type of fields that define the annotated union.</param>
              /// <param name="isPubliclyMutable">Determines if mutability is private or public.</param>
              internal ChoiceAttribute(Type set, bool isPubliclyMutable)
              {
                  Set = set;
                  IsPubliclyMutable = isPubliclyMutable;
              }
              
              /// <summary>
              /// Initializes a new instance of the <see cref="ChoiceAttribute"/> class.
              /// </summary>
              /// <param name="isPubliclyMutable">Determines if mutability is private or public.</param>
              /// <param name="set">The type of fields that define the annotated union.</param>
              internal ChoiceAttribute(bool isPubliclyMutable, Type set)
              {
                  IsPubliclyMutable = isPubliclyMutable;
                  Set = set;
              }
      
              /// <summary>
              /// When specified, determines whether the type is mutable publicly.
              /// </summary>
              internal bool? IsPubliclyMutable { get; }
              
              /// <summary>
              /// When specified, gets the set of fields that define the annotated union.
              /// </summary>
              internal Type? Set { get; }
          }
      }
      """
);
