grammar SPSL
  ;

options {
  language = CSharp;
}

@header {
using SPSL.Language.Parsing.AST;
using SPSL.Language.Parsing.Common;

namespace SPSL.Language.Core;
}

KEYWORD_NAMESPACE
  : 'namespace'
  ;
KEYWORD_ABSTRACT
  : 'abstract'
  ;
KEYWORD_VERTEX
  : 'vertex'
  ;
KEYWORD_FRAGMENT
  : 'fragment'
  ;
KEYWORD_PIXEL
  : 'pixel'
  ;
KEYWORD_GEOMETRY
  : 'geometry'
  ;
KEYWORD_HULL
  : 'hull'
  ;
KEYWORD_DOMAIN
  : 'domain'
  ;
KEYWORD_COMPUTE
  : 'compute'
  ;
KEYWORD_GRAPHIC
  : 'graphic'
  ;
KEYWORD_SHADER
  : 'shader'
  ;
KEYWORD_INTERFACE
  : 'interface'
  ;
KEYWORD_MATERIAL
  : 'material'
  ;
KEYWORD_PARTIAL
  : 'partial'
  ;
KEYWORD_PARAMS
  : 'params'
  ;
KEYWORD_TYPE
  : 'type'
  ;
KEYWORD_AS
  : 'as'
  ;
KEYWORD_IN
  : 'in'
  ;
KEYWORD_OF
  : 'of'
  ;
KEYWORD_OUT
  : 'out'
  ;
KEYWORD_INOUT
  : 'inout'
  ;
KEYWORD_EXTENDS
  : 'extends'
  ;
KEYWORD_IMPLEMENTS
  : 'implements'
  ;
KEYWORD_USE
  : 'use'
  ;
KEYWORD_VAR
  : 'var'
  ;
KEYWORD_INPUT
  : 'input'
  ;
KEYWORD_OUTPUT
  : 'output'
  ;
KEYWORD_TRANSIENT
  : 'transient'
  ;
KEYWORD_BUFFER
  : 'buffer'
  ;
KEYWORD_OVERRIDE
  : 'override'
  ;
KEYWORD_THIS
  : 'this'
  ;
KEYWORD_BASE
  : 'base'
  ;
KEYWORD_STRUCT
  : 'struct'
  ;
KEYWORD_ENUM
  : 'enum'
  ;
KEYWORD_RETURN
  : 'return'
  ;
KEYWORD_IF
  : 'if'
  ;
KEYWORD_ELIF
  : 'elif'
  ;
KEYWORD_ELSE
  : 'else'
  ;
KEYWORD_SWITCH
  : 'switch'
  ;
KEYWORD_FOR
  : 'for'
  ;
KEYWORD_DO
  : 'do'
  ;
KEYWORD_WHILE
  : 'while'
  ;
KEYWORD_BREAK
  : 'break'
  ;
KEYWORD_CONTINUE
  : 'continue'
  ;
KEYWORD_DISCARD
  : 'discard'
  ;
KEYWORD_CASE
  : 'case'
  ;
KEYWORD_STATIC
  : 'static'
  ;
KEYWORD_CONST
  : 'const'
  ;
KEYWORD_PERMUTATION
  : 'permutation'
  ;
KEYWORD_VARIANT
  : 'variant'
  ;
KEYWORD_PERMUTE
  : 'permute'
  ;
KEYWORD_STREAM
  : 'stream'
  ;
KEYWORD_COHERENT
  : 'coherent'
  ;
KEYWORD_VOLATILE
  : 'volatile'
  ;
KEYWORD_READONLY
  : 'readonly'
  ;
KEYWORD_WRITEONLY
  : 'writeonly'
  ;
KEYWORD_READWRITE
  : 'readwrite'
  ;

TYPE_VOID
  : 'void'
  ;
TYPE_BOOL
  : 'bool'
  ;
TYPE_INT
  : 'int'
  ;
TYPE_UINT
  : 'uint'
  ;
TYPE_FLOAT
  : 'float'
  ;
TYPE_DOUBLE
  : 'double'
  ;
TYPE_STRING
  : 'string'
  ;
TYPE_VECTOR2B
  : 'vec2b'
  ;
TYPE_VECTOR2D
  : 'vec2d'
  ;
TYPE_VECTOR2F
  : 'vec2f'
  ;
TYPE_VECTOR2I
  : 'vec2i'
  ;
TYPE_VECTOR2UI
  : 'vec2ui'
  ;
TYPE_VECTOR3B
  : 'vec3b'
  ;
TYPE_VECTOR3D
  : 'vec3d'
  ;
TYPE_VECTOR3F
  : 'vec3f'
  ;
TYPE_VECTOR3I
  : 'vec3i'
  ;
TYPE_VECTOR3UI
  : 'vec3ui'
  ;
TYPE_VECTOR4B
  : 'vec4b'
  ;
TYPE_VECTOR4D
  : 'vec4d'
  ;
TYPE_VECTOR4F
  : 'vec4f'
  ;
TYPE_VECTOR4I
  : 'vec4i'
  ;
TYPE_VECTOR4UI
  : 'vec4ui'
  ;
TYPE_MATRIX2F
  : 'mat2f'
  ;
TYPE_MATRIX3F
  : 'mat3f'
  ;
TYPE_MATRIX4F
  : 'mat4f'
  ;
TYPE_MATRIX2X3F
  : 'mat2x3f'
  ;
TYPE_MATRIX2X4F
  : 'mat2x4f'
  ;
TYPE_MATRIX3X2F
  : 'mat3x2f'
  ;
TYPE_MATRIX3X4F
  : 'mat3x4f'
  ;
TYPE_MATRIX4X2F
  : 'mat4x2f'
  ;
TYPE_MATRIX4X3F
  : 'mat4x3f'
  ;
TYPE_MATRIX2D
  : 'mat2d'
  ;
TYPE_MATRIX3D
  : 'mat3d'
  ;
TYPE_MATRIX4D
  : 'mat4d'
  ;
TYPE_MATRIX2X3D
  : 'mat2x3d'
  ;
TYPE_MATRIX2X4D
  : 'mat2x4d'
  ;
TYPE_MATRIX3X2D
  : 'mat3x2d'
  ;
TYPE_MATRIX3X4D
  : 'mat3x4d'
  ;
TYPE_MATRIX4X2D
  : 'mat4x2d'
  ;
TYPE_MATRIX4X3D
  : 'mat4x3d'
  ;
TYPE_COLOR3
  : 'color3'
  ;
TYPE_COLOR4
  : 'color4'
  ;
TYPE_SAMPLER
  : 'sampler'
  ;
TYPE_TEXTURE1D
  : 'tex1d'
  ;
TYPE_TEXTURE2D
  : 'tex2d'
  ;
TYPE_TEXTURE1DARRAY
  : 'arraytex1d'
  ;
TYPE_TEXTURE2DARRAY
  : 'arraytex2d'
  ;
TYPE_TEXTURE3D
  : 'tex3d'
  ;
TYPE_CUBEMAP
  : 'texcube'
  ;
TYPE_CUBEMAPARRAY
  : 'arraytexcube'
  ;

TOK_OPEN_PAREN
  : '('
  ;
TOK_CLOSE_PAREN
  : ')'
  ;
TOK_OPEN_BRACKET
  : '['
  ;
TOK_CLOSE_BRACKET
  : ']'
  ;
TOK_OPEN_BRACE
  : '{'
  ;
TOK_CLOSE_BRACE
  : '}'
  ;
TOK_DOT
  : '.'
  ;
TOK_UNDERSCORE
  : '_'
  ;
TOK_TILDE
  : '~'
  ;
TOK_QUESTION
  : '?'
  ;
TOK_COLON
  : ':'
  ;
TOK_SEMICOLON
  : ';'
  ;
TOK_COMMA
  : ','
  ;
TOK_HASHTAG
  : '#'
  ;
TOK_DOLLAR
  : '$'
  ;
TOK_AT
  : '@'
  ;
TOK_BACKSLASH
  : '\\'
  ;
TOK_NAMESPACE_SEPARATOR
  : '::'
  ;

OP_PIPE
  : '|'
  ;
OP_AMPERSAND
  : '&'
  ;
OP_PLUS
  : '+'
  ;
OP_MINUS
  : '-'
  ;
OP_ASTERISK
  : '*'
  ;
OP_EXPONENT
  : '^'
  ;
OP_MODULUS
  : '%'
  ;
OP_DIV
  : '/'
  ;
OP_ASSIGN
  : '='
  ;
OP_EQUAL
  : '=='
  ;
OP_DIFFERENT
  : '!='
  ;
OP_GREATER_THAN
  : '>'
  ;
OP_LESSER_THAN
  : '<'
  ;
OP_GEQ_THAN
  : '>='
  ;
OP_LEQ_THAN
  : '<='
  ;
OP_INCREMENT
  : '++'
  ;
OP_DECREMENT
  : '--'
  ;
OP_PLUS_ASSIGN
  : '+='
  ;
OP_MINUS_ASSIGN
  : '-='
  ;
OP_MUL_ASSIGN
  : '*='
  ;
OP_DIV_ASSIGN
  : '/='
  ;
OP_MODULUS_ASSIGN
  : '%='
  ;
OP_BITWISE_OR_ASSIGN
  : '|='
  ;
OP_BITWISE_AND_ASSIGN
  : '&='
  ;
OP_EXPONENT_ASSIGN
  : '^='
  ;
OP_LSHIFT_ASSIGN
  : '<<='
  ;
OP_RSHIFT_ASSIGN
  : '>>='
  ;
OP_OR
  : '||'
  | 'or'
  ;
OP_AND
  : '&&'
  | 'and'
  ;
OP_NOT
  : '!'
  | 'not'
  ;
OP_XOR
  : '^^'
  ;
OP_LSHIFT
  : '<<'
  ;
OP_RSHIFT
  : '>>'
  ;

// ========== Parser rules

shaderFile
  : DOC_COMMENT? Directives = directive* Namespace = namespaceDefinition? (DOC_COMMENT* useNamespaceDirective)* FileLevelDefinitions = fileLevelDefinition* EOF
  ;

materialFile
  : DOC_COMMENT? Directives = directive* Namespace = namespaceDefinition? (DOC_COMMENT* useNamespaceDirective)* Material = material EOF
  ;

namespaceDefinition
  : KEYWORD_NAMESPACE Name = namespacedTypeName TOK_SEMICOLON
  ;

namespacedTypeName
  : Name += IDENTIFIER (TOK_NAMESPACE_SEPARATOR Name += IDENTIFIER)*
  ;

fileLevelDefinition
  : permutationVariable
  | type
  | interface
  | shaderFragment
  | shader
  ;

globalVariable
  locals[bool IsStatic]
  : Documentation = DOC_COMMENT? (KEYWORD_STATIC {$IsStatic = true;})? KEYWORD_CONST Type = dataType Definition = variableDeclarationAssignment TOK_SEMICOLON
  ;

permutationVariableBool
  : Documentation = DOC_COMMENT? KEYWORD_PERMUTATION TYPE_BOOL Identifier = basicExpression OP_ASSIGN Value = BoolLiteral TOK_SEMICOLON
  ;

permutationVariableEnum
  : Documentation = DOC_COMMENT? KEYWORD_PERMUTATION KEYWORD_ENUM Identifier = basicExpression TOK_OPEN_BRACE EnumValues += IDENTIFIER (
    TOK_COMMA EnumValues += IDENTIFIER
  )* TOK_CLOSE_BRACE OP_ASSIGN Value = basicExpression TOK_SEMICOLON
  ;

permutationVariableInteger
  : Documentation = DOC_COMMENT? Annotations += annotation* KEYWORD_PERMUTATION TYPE_INT Identifier = basicExpression OP_ASSIGN Value = IntegerLiteral TOK_SEMICOLON
  ;

permutationVariable
  : permutationVariableBool
  | permutationVariableEnum
  | permutationVariableInteger
  ;

materialVariant
  : Documentation = DOC_COMMENT? KEYWORD_VARIANT Name = IDENTIFIER TOK_OPEN_BRACE Values += materialVariantValue* TOK_CLOSE_BRACE
  ;

materialVariantValue
  : Name = IDENTIFIER OP_ASSIGN Value = constantExpression TOK_SEMICOLON
  ;

type
  : Documentation = DOC_COMMENT? Definition = structDefinition TOK_OPEN_BRACE structComponent* TOK_CLOSE_BRACE # Struct
  | Documentation = DOC_COMMENT? Definition = enumDefinition TOK_OPEN_BRACE enumComponent* TOK_CLOSE_BRACE     # Enum
  ;

structDefinition
  : KEYWORD_TYPE Name = IDENTIFIER KEYWORD_AS KEYWORD_STRUCT (KEYWORD_EXTENDS ExtendedType = namespacedTypeName)?
  ;

enumDefinition
  : KEYWORD_TYPE Name = IDENTIFIER KEYWORD_AS KEYWORD_ENUM
  ;

interface
  : Documentation = DOC_COMMENT? Definition = interfaceDefinition TOK_OPEN_BRACE (functionHead TOK_SEMICOLON)* TOK_CLOSE_BRACE
  ;

interfaceDefinition
  : KEYWORD_INTERFACE Name = IDENTIFIER (KEYWORD_EXTENDS ExtendedInterfaces = interfacesList)?
  ;

interfacesList
  : namespacedTypeName (TOK_COMMA namespacedTypeName)*
  ;

shaderFragment
  : Documentation = DOC_COMMENT? Definition = shaderFragmentDefinition TOK_OPEN_BRACE (
    shaderMember
    | permutationVariable
  )* TOK_CLOSE_BRACE
  ;

shaderFragmentDefinition
  : KEYWORD_FRAGMENT Name = IDENTIFIER (KEYWORD_EXTENDS ExtendedFragment = namespacedTypeName)? (
    KEYWORD_IMPLEMENTS ExtendedInterfaces = interfacesList
  )?
  ;

shader
  : Documentation = DOC_COMMENT? Definition = shaderDefinition TOK_OPEN_BRACE shaderMember* TOK_CLOSE_BRACE
  ;

material
  : Documentation = DOC_COMMENT? Definition = materialDefinition TOK_OPEN_BRACE (
    DOC_COMMENT* (materialMember | useFragmentDirective)
  )* TOK_CLOSE_BRACE
  ;

stream
  locals[bool IsPartial]
  : Documentation = DOC_COMMENT? KEYWORD_STREAM TOK_OPEN_BRACE (DOC_COMMENT* streamProperty)* TOK_CLOSE_BRACE
  ;

materialDefinition
  locals[bool IsAbstract]
  : (KEYWORD_ABSTRACT {$IsAbstract = true;})? KEYWORD_MATERIAL Name = IDENTIFIER (
    KEYWORD_EXTENDS ExtendedMaterial = namespacedTypeName
  )?
  ;

shaderDefinition
  locals[bool IsAbstract]
  : (KEYWORD_ABSTRACT {$IsAbstract = true;})? (
    Type = (KEYWORD_COMPUTE | KEYWORD_VERTEX | KEYWORD_PIXEL | KEYWORD_GEOMETRY | KEYWORD_HULL | KEYWORD_DOMAIN)
  )? KEYWORD_SHADER Name = IDENTIFIER (KEYWORD_EXTENDS ExtendedShader = namespacedTypeName)? (
    KEYWORD_IMPLEMENTS Interfaces = interfacesList
  )? # GenericShaderDefinition
  | Type = KEYWORD_COMPUTE KEYWORD_SHADER Name = IDENTIFIER OP_LESSER_THAN ThreadCountX = IntegerLiteral TOK_COMMA ThreadCountY = IntegerLiteral TOK_COMMA ThreadCountZ =
    IntegerLiteral OP_GREATER_THAN (KEYWORD_EXTENDS ExtendedShader = namespacedTypeName)? (
    KEYWORD_IMPLEMENTS Interfaces = interfacesList
  )? # ComputeShaderDefinition
  ;

useNamespaceDirective
  : KEYWORD_USE KEYWORD_NAMESPACE Name = namespacedTypeName TOK_SEMICOLON
  ;

useFragmentDirective
  : KEYWORD_USE KEYWORD_FRAGMENT Name = namespacedTypeName TOK_SEMICOLON
  ;

streamProperty
  : annotation* Flow = (KEYWORD_INPUT | KEYWORD_OUTPUT | KEYWORD_TRANSIENT) bufferComponent
  ;

shaderMember
  : globalVariable
  | bufferDefinition
  | type
  | stream
  | samplerState
  | shaderFunction
  | useFragmentDirective
  ;

samplerState
  : Documentation = DOC_COMMENT? TYPE_SAMPLER Name = IDENTIFIER OP_ASSIGN IDENTIFIER                                 # DefaultSamplerState
  | Documentation = DOC_COMMENT? TYPE_SAMPLER Name = IDENTIFIER TOK_OPEN_BRACE samplerStateProperty+ TOK_CLOSE_BRACE # CustomSamplerState
  ;

samplerStateProperty
  : Documentation = DOC_COMMENT? Property = IDENTIFIER OP_ASSIGN Value = primitiveExpression
  ;

materialMember
  : materialParams
  | materialState
  | type
  | shaderFunction
  | materialShaderUsage
  | materialVariant
  ;

materialShaderUsageDefinition
  : KEYWORD_USE Stage = (KEYWORD_VERTEX | KEYWORD_PIXEL | KEYWORD_GEOMETRY | KEYWORD_DOMAIN | KEYWORD_HULL)? KEYWORD_SHADER Name = namespacedTypeName
  ;

materialShaderUsage
  : Definition = materialShaderUsageDefinition TOK_SEMICOLON # SimpleMaterialShaderUsage
  | Definition = materialShaderUsageDefinition (KEYWORD_AS Name = IDENTIFIER)? TOK_OPEN_BRACE (
    DOC_COMMENT* (shaderFunction | useFragmentDirective)
  )* TOK_CLOSE_BRACE # CustomizedMaterialShaderUsage
  ;

// ----- Annotations -----
annotation
  : TOK_AT Name = IDENTIFIER (TOK_OPEN_PAREN (constantExpression ( TOK_COMMA constantExpression)*)? TOK_CLOSE_PAREN)?
  ;

// ----------------------

// ----- Directives -----

directive
  : precisionDirective
  | shaderLangDirective
  ;

precisionDirective
  : TOK_HASHTAG 'precision' ('low' | 'medium' | 'high')
  ;

shaderLangDirective
  : TOK_HASHTAG 'shaderLang' ('GLSL' | 'HLSL' | 'MSL')
  ;

// ----------------------

materialParams
  locals[bool IsPartial]
  : annotation* (KEYWORD_PARTIAL {$IsPartial = true;})? KEYWORD_PARAMS Name = IDENTIFIER TOK_OPEN_BRACE materialParamsComponent* TOK_CLOSE_BRACE
  ;

materialState
  : 'state' Name = IDENTIFIER TOK_OPEN_BRACE materialStateComponent* TOK_CLOSE_BRACE # MaterialStateBlock
  | 'state' Name = IDENTIFIER OP_ASSIGN Value = IDENTIFIER TOK_SEMICOLON             # MaterialStateValue
  ;

materialStateComponent
  : Name = IDENTIFIER OP_ASSIGN Value = initializationExpression TOK_SEMICOLON
  ;

// Uniform block for GLSL CBuffer for HLSL
bufferDefinition
  : Documentation = DOC_COMMENT? annotation* Storage = KEYWORD_COHERENT? Access = (
    KEYWORD_READONLY
    | KEYWORD_WRITEONLY
    | KEYWORD_READWRITE
    | KEYWORD_CONST
  )? KEYWORD_BUFFER Name = IDENTIFIER TOK_OPEN_BRACE bufferComponent* TOK_CLOSE_BRACE # InPlaceStructuredBufferDefinition
  | Documentation = DOC_COMMENT? annotation* Storage = KEYWORD_COHERENT? Access = (
    KEYWORD_READONLY
    | KEYWORD_WRITEONLY
    | KEYWORD_READWRITE
  )? KEYWORD_BUFFER OP_LESSER_THAN Type = dataType OP_GREATER_THAN Name = IDENTIFIER TOK_SEMICOLON # TypedBufferDefinition
  ;

bufferComponent
  : annotation* Type = dataType Name = IDENTIFIER TOK_SEMICOLON
  ;

materialParamsComponent
  : annotation* Type = dataType Name = IDENTIFIER (OP_ASSIGN DefaultValue = initializationExpression)? TOK_SEMICOLON # MaterialValueParameter
  | permutationVariable                                                                                              # MaterialPermutationParameter
  ;

structComponent
  : Documentation = DOC_COMMENT? annotation* Type = dataType Name = IDENTIFIER TOK_SEMICOLON # StructProperty
  | annotation* Function = function                                                          # StructFunction
  ;

enumComponent
  : Documentation = DOC_COMMENT? annotation* Name = IDENTIFIER (OP_ASSIGN Value = constantExpression)? TOK_COMMA
  ;

// Inner function variables
variableDeclaration
  locals[bool IsConst]
  : (KEYWORD_CONST {$IsConst = true;})? Type = dataType variableIdentity (TOK_COMMA variableIdentity)* # TypedVariableDeclaration
  |                {$IsConst = true;} KEYWORD_CONST KEYWORD_VAR Identifier = basicExpression OP_ASSIGN Expression = primitiveExpression # UntypedVariableDeclaration
  ;

variableDeclarationAssignment
  : Identifier = basicExpression OP_ASSIGN Expression = expressionStatement
  ;

variableIdentity
  locals[bool IsAssignment]
  : Identifier = basicExpression
  | Declaration = variableDeclarationAssignment {$IsAssignment = true;}
  ;

shaderFunction
  locals[bool IsOverride]
  : Documentation = DOC_COMMENT? annotation* (KEYWORD_OVERRIDE                                                                                   {$IsOverride = true;})? Function = function # BasicShaderFunction
  | Documentation = DOC_COMMENT? annotation* Name = IDENTIFIER TOK_OPEN_PAREN TOK_CLOSE_PAREN TOK_OPEN_BRACE Body = functionBody TOK_CLOSE_BRACE # ShaderConstructorFunction
  ;

function
  : Head = functionHead TOK_OPEN_BRACE Body = functionBody TOK_CLOSE_BRACE
  ;

functionHead
  : Documentation = DOC_COMMENT? Type = dataType Name = IDENTIFIER Signature = functionSignature
  ;

functionSignature
  : TOK_OPEN_PAREN Arguments = argList? TOK_CLOSE_PAREN
  ;

argList
  : Arguments += argDef (TOK_COMMA Arguments += argDef)*
  ;

argDef
  : Documentation = DOC_COMMENT? Annotations += annotation* Flow = (
    KEYWORD_IN
    | KEYWORD_OUT
    | KEYWORD_INOUT
    | KEYWORD_CONST
  )? Type = dataType Name = IDENTIFIER (OP_ASSIGN DefaultValue = constantExpression)?
  ;

functionBody
  : stayControlFlowStatement* ReturnStatement = returnStatement?
  ;

statementBlock
  : TOK_OPEN_BRACE statement* TOK_CLOSE_BRACE
  ;

referencableExpression
  : basicExpression
  | parenthesizedExpression
  | invocationExpression
  | propertyMemberReferenceExpression
  | methodMemberReferenceExpression
  | arrayAccessExpression
  ;

chainableExpression
  : basicExpression
  | invocationExpression
  | arrayAccessExpression
  ;

chainedExpression
  : Target = referencableExpression (TOK_DOT chainableExpression)+
  ;

assignableChainableExpression
  : basicExpression
  | arrayAccessExpression
  ;

assignableChainedExpression
  : Target = referencableExpression (TOK_DOT assignableChainableExpression)+
  ;

propertyMemberReferenceExpression
  : Target = contextAccessExpression TOK_DOT Member = basicExpression
  ;

methodMemberReferenceExpression
  : Target = contextAccessExpression TOK_DOT Member = invocationExpression
  ;

memberReferenceExpression
  : propertyMemberReferenceExpression
  | methodMemberReferenceExpression
  ;

invocationExpression
  : Name = namespacedTypeName TOK_OPEN_PAREN Parameters = parametersList? TOK_CLOSE_PAREN
  ;

permuteStatement
  : KEYWORD_PERMUTE TOK_OPEN_PAREN Identifier = basicExpression Operator = (
    OP_EQUAL
    | OP_GREATER_THAN
    | OP_GEQ_THAN
    | OP_LESSER_THAN
    | OP_LEQ_THAN
    | OP_DIFFERENT
  ) Value = constantExpression TOK_CLOSE_PAREN Block = statementBlock Else = elseStatement?
  ;

ifStatement
  : KEYWORD_IF TOK_OPEN_PAREN Expression = expressionStatement TOK_CLOSE_PAREN Block = statementBlock elifStatement* Else = elseStatement?
  ;

elifStatement
  : (KEYWORD_ELIF | KEYWORD_ELSE KEYWORD_IF) TOK_OPEN_PAREN Expression = expressionStatement TOK_CLOSE_PAREN Block = statementBlock
  ;

elseStatement
  : KEYWORD_ELSE Block = statementBlock
  ;

switchStatement
  : KEYWORD_SWITCH TOK_OPEN_PAREN Expression = expressionStatement TOK_CLOSE_PAREN TOK_OPEN_BRACE caseStatement* TOK_CLOSE_BRACE
  ;

caseStatement
  locals[bool IsBraceOpen = false, bool IsBraceClose = false]
  : KEYWORD_CASE Expression = constantExpression TOK_COLON (TOK_OPEN_BRACE {$IsBraceOpen = true;})? Statements = stayControlFlowStatement* leaveControlFlowStatement (
    TOK_CLOSE_BRACE                                                        {$IsBraceClose = true;}
  )?                                                                       {$IsBraceOpen == $IsBraceClose}?
  ;

whileStatement
  : KEYWORD_WHILE TOK_OPEN_PAREN Expression = expressionStatement TOK_CLOSE_PAREN Block = statementBlock
  ;

forStatement
  : KEYWORD_FOR TOK_OPEN_PAREN Initialization = expressionStatement TOK_SEMICOLON Condition = expressionStatement TOK_SEMICOLON Iteration = expressionStatement TOK_CLOSE_PAREN
    Block = statementBlock
  ;

doWhileStatement
  : KEYWORD_DO Block = statementBlock KEYWORD_WHILE Expression = parenthesizedExpression TOK_SEMICOLON
  ;

parenthesizedExpression
  : TOK_OPEN_PAREN Expression = expressionStatement TOK_CLOSE_PAREN
  ;

newInstanceExpression
  : Type = languageDataType TOK_OPEN_PAREN Parameters = parametersList? TOK_CLOSE_PAREN
  ;

parametersList
  : expressionStatement (TOK_COMMA expressionStatement)*
  ;

contextAccessExpression
  : Identifier = (KEYWORD_BASE | KEYWORD_THIS)
  ;

basicExpression
  : Identifier = IDENTIFIER
  ;

initializationExpression
  : basicExpression
  | primitiveExpression
  | newInstanceExpression
  ;

expressionStatement
  : basicExpression                                                                                                                       # Expression
  | primitiveExpression                                                                                                                   # Expression
  | newInstanceExpression                                                                                                                 # Expression
  | parenthesizedExpression                                                                                                               # Expression
  | propertyMemberReferenceExpression                                                                                                     # Expression
  | methodMemberReferenceExpression                                                                                                       # Expression
  | chainedExpression                                                                                                                     # Expression
  | invocationExpression                                                                                                                  # Expression
  | arrayAccessExpression                                                                                                                 # Expression
  | Expression = assignableExpression Operator = (OP_INCREMENT | OP_DECREMENT)                                                            # PostfixUnaryOperationExpression
  | <assoc = right> Operator = (OP_INCREMENT | OP_DECREMENT) Expression = assignableExpression                                            # PrefixUnaryOperationExpression
  | <assoc = right> OP_NOT Expression = expressionStatement                                                                               # NegateOperationExpression
  | <assoc = right> Operator = (OP_MINUS | OP_PLUS) Expression = expressionStatement                                                      # SignedExpression
  | Left = expressionStatement Operator = OP_LEQ_THAN Right = expressionStatement                                                         # BinaryOperationExpression
  | Left = expressionStatement Operator = OP_GEQ_THAN Right = expressionStatement                                                         # BinaryOperationExpression
  | Left = expressionStatement Operator = OP_LESSER_THAN Right = expressionStatement                                                      # BinaryOperationExpression
  | Left = expressionStatement Operator = OP_GREATER_THAN Right = expressionStatement                                                     # BinaryOperationExpression
  | Left = expressionStatement Operator = OP_EQUAL Right = expressionStatement                                                            # BinaryOperationExpression
  | Left = expressionStatement Operator = OP_DIFFERENT Right = expressionStatement                                                        # BinaryOperationExpression
  | Left = expressionStatement Operator = OP_PLUS Right = expressionStatement                                                             # BinaryOperationExpression
  | Left = expressionStatement Operator = OP_MINUS Right = expressionStatement                                                            # BinaryOperationExpression
  | Left = expressionStatement Operator = OP_DIV Right = expressionStatement                                                              # BinaryOperationExpression
  | Left = expressionStatement Operator = OP_MODULUS Right = expressionStatement                                                          # BinaryOperationExpression
  | Left = expressionStatement Operator = OP_ASTERISK Right = expressionStatement                                                         # BinaryOperationExpression
  | Left = expressionStatement Operator = OP_PIPE Right = expressionStatement                                                             # BinaryOperationExpression
  | Left = expressionStatement Operator = OP_AMPERSAND Right = expressionStatement                                                        # BinaryOperationExpression
  | <assoc = right> Left = expressionStatement Operator = OP_EXPONENT Right = expressionStatement                                         # BinaryOperationExpression
  | <assoc = right> Left = expressionStatement Operator = OP_LSHIFT Right = expressionStatement                                           # BinaryOperationExpression
  | <assoc = right> Left = expressionStatement Operator = OP_RSHIFT Right = expressionStatement                                           # BinaryOperationExpression
  | Left = expressionStatement Operator = OP_AND Right = expressionStatement                                                              # BinaryOperationExpression
  | Left = expressionStatement Operator = OP_OR Right = expressionStatement                                                               # BinaryOperationExpression
  | Left = expressionStatement Operator = OP_XOR Right = expressionStatement                                                              # BinaryOperationExpression
  | <assoc = right> Condition = expressionStatement TOK_QUESTION WhenTrue = expressionStatement TOK_COLON WhenFalse = expressionStatement # TernaryOperationExpression
  | <assoc = right> Left = assignableExpression Operator = OP_ASSIGN Right = expressionStatement                                          # AssignmentExpression
  | <assoc = right> Left = assignableExpression Operator = OP_PLUS_ASSIGN Right = expressionStatement                                     # AssignmentExpression
  | <assoc = right> Left = assignableExpression Operator = OP_MINUS_ASSIGN Right = expressionStatement                                    # AssignmentExpression
  | <assoc = right> Left = assignableExpression Operator = OP_MUL_ASSIGN Right = expressionStatement                                      # AssignmentExpression
  | <assoc = right> Left = assignableExpression Operator = OP_DIV_ASSIGN Right = expressionStatement                                      # AssignmentExpression
  | <assoc = right> Left = assignableExpression Operator = OP_MODULUS_ASSIGN Right = expressionStatement                                  # AssignmentExpression
  | <assoc = right> Left = assignableExpression Operator = OP_BITWISE_OR_ASSIGN Right = expressionStatement                               # AssignmentExpression
  | <assoc = right> Left = assignableExpression Operator = OP_BITWISE_AND_ASSIGN Right = expressionStatement                              # AssignmentExpression
  | <assoc = right> Left = assignableExpression Operator = OP_EXPONENT_ASSIGN Right = expressionStatement                                 # AssignmentExpression
  | <assoc = right> Left = assignableExpression Operator = OP_LSHIFT_ASSIGN Right = expressionStatement                                   # AssignmentExpression
  | <assoc = right> Left = assignableExpression Operator = OP_RSHIFT_ASSIGN Right = expressionStatement                                   # AssignmentExpression
  | TOK_OPEN_PAREN Type = dataType TOK_CLOSE_PAREN Expression = expressionStatement                                                       # CastExpression
  ;

arrayAccessExpression
  : (basicExpression | memberReferenceExpression | invocationExpression) TOK_OPEN_BRACKET Index = expressionStatement TOK_CLOSE_BRACKET
  ;

assignableExpression
  : basicExpression
  | arrayAccessExpression
  | propertyMemberReferenceExpression
  | assignableChainedExpression
  ;

returnStatement
  : KEYWORD_RETURN Expression = expressionStatement? TOK_SEMICOLON
  ;

breakStatement
  : KEYWORD_BREAK TOK_SEMICOLON
  ;

continueStatement
  : KEYWORD_CONTINUE TOK_SEMICOLON
  ;

discardStatement
  : KEYWORD_DISCARD TOK_SEMICOLON
  ;

leaveControlFlowStatement
  : BreakStatement = breakStatement
  | ReturnStatement = returnStatement
  | ContinueStatement = continueStatement
  | DiscardStatement = discardStatement
  ;

stayControlFlowStatement
  : VariableDeclaration = variableDeclaration TOK_SEMICOLON
  | ExpressionStatement = expressionStatement TOK_SEMICOLON
  | StatementBlock = statementBlock
  | IfStatement = ifStatement
  | SwitchStatement = switchStatement
  | WhileStatement = whileStatement
  | ForStatement = forStatement
  | DoWhileStatement = doWhileStatement
  | PermuteStatement = permuteStatement
  ;

statement
  : StayControlFlowStatement = stayControlFlowStatement
  | LeaveControlFlowStatement = leaveControlFlowStatement
  ;

primitiveExpression
  : Literal = BoolLiteral
  | Literal = DoubleLiteral
  | Literal = FloatLiteral
  | Literal = IntegerLiteral
  | Literal = UnsignedIntegerLiteral
  | Literal = StringLiteral
  ;

constantExpression
  : primitiveExpression # PrimitiveConstantExpression
  | namespacedTypeName  # UserDefinedConstantExpression
  ;

// ----- Types -----
languageDataType
  locals[bool IsArray, DataTypeKind DataType]
  : primitiveDataType (TOK_OPEN_BRACKET ArraySize = IntegerLiteral? TOK_CLOSE_BRACKET {$IsArray = true;})? {$DataType = DataTypeKind.Primitive;}
  | builtinDataType (TOK_OPEN_BRACKET ArraySize = IntegerLiteral? TOK_CLOSE_BRACKET   {$IsArray = true;})? {$DataType = DataTypeKind.BuiltIn;}
  ;

customDataType
  locals[bool IsArray, DataTypeKind DataType]
  : userDefinedDataType (TOK_OPEN_BRACKET ArraySize = IntegerLiteral? TOK_CLOSE_BRACKET {$IsArray = true;})? {$DataType = DataTypeKind.UserDefined;}
  ;

dataType
  : languageDataType
  | customDataType
  ;

userDefinedDataType
  : Type = namespacedTypeName
  ;

primitiveDataType
  : Type = TYPE_VOID
  | Type = TYPE_BOOL
  | Type = TYPE_INT
  | Type = TYPE_UINT
  | Type = TYPE_FLOAT
  | Type = TYPE_DOUBLE
  | Type = TYPE_STRING
  ;

builtinDataType
  : Type = TYPE_VECTOR2B
  | Type = TYPE_VECTOR2F
  | Type = TYPE_VECTOR2D
  | Type = TYPE_VECTOR2I
  | Type = TYPE_VECTOR2UI
  | Type = TYPE_VECTOR3B
  | Type = TYPE_VECTOR3F
  | Type = TYPE_VECTOR3D
  | Type = TYPE_VECTOR3I
  | Type = TYPE_VECTOR3UI
  | Type = TYPE_VECTOR4B
  | Type = TYPE_VECTOR4F
  | Type = TYPE_VECTOR4D
  | Type = TYPE_VECTOR4I
  | Type = TYPE_VECTOR4UI
  | Type = TYPE_MATRIX2F
  | Type = TYPE_MATRIX2D
  | Type = TYPE_MATRIX3F
  | Type = TYPE_MATRIX3D
  | Type = TYPE_MATRIX4F
  | Type = TYPE_MATRIX4D
  | Type = TYPE_MATRIX2X3F
  | Type = TYPE_MATRIX2X3D
  | Type = TYPE_MATRIX2X4F
  | Type = TYPE_MATRIX2X4D
  | Type = TYPE_MATRIX3X2F
  | Type = TYPE_MATRIX3X2D
  | Type = TYPE_MATRIX3X4F
  | Type = TYPE_MATRIX3X4D
  | Type = TYPE_MATRIX4X2F
  | Type = TYPE_MATRIX4X2D
  | Type = TYPE_MATRIX4X3F
  | Type = TYPE_MATRIX4X3D
  | Type = TYPE_COLOR3
  | Type = TYPE_COLOR4
  | Type = TYPE_SAMPLER
  | Type = TYPE_TEXTURE1D
  | Type = TYPE_TEXTURE1DARRAY
  | Type = TYPE_TEXTURE2D
  | Type = TYPE_TEXTURE2DARRAY
  | Type = TYPE_TEXTURE3D
  | Type = TYPE_CUBEMAP
  | Type = TYPE_CUBEMAPARRAY
  ;

// --------------------

// --------------- Whitespace ---------------

WHITESPACE
  : Ws+ -> skip
  ;

// --------------- Comments ---------------

DOC_COMMENT
  : DocComment
  ;

BLOCK_COMMENT
  : BlockComment -> channel (HIDDEN)
  ;

LINE_COMMENT
  : LineComment -> channel (HIDDEN)
  ;

// --------------- Literals ---------------

BoolLiteral
  : 'true'
  | 'false'
  ;

DoubleLiteral
  : (OP_PLUS | OP_MINUS)? (FractionalConstant DoubleSuffix? | DecimalDigit+ DoubleSuffix)
  ;

FloatLiteral
  : (OP_PLUS | OP_MINUS)? (FractionalConstant ExponentPart? FloatingSuffix | DecimalDigit+ ExponentPart FloatingSuffix)
  ;

IntegerLiteral
  : (OP_PLUS | OP_MINUS)? (DecimalConstant | OctalConstant | HexadecimalConstant)
  ;

UnsignedIntegerLiteral
  : OP_PLUS? DecimalConstant UnsingedIntegerSuffix
  ;

StringLiteral
  : '"' .*? '"'
  ;

// --------------- Identifiers ---------------

IDENTIFIER
  : NonDigit (NonDigit | DecimalDigit)*
  ;

// --------------- Whitespace and comments ---------------

fragment Ws
  : Hws
  | Vws
  ;

fragment Hws
  : [ \t]
  ;

fragment Vws
  : [\r\n\f]
  ;

fragment BlockComment
  : '/*' .*? ('*/' | EOF)
  ;

fragment DocComment
  : '/**' .*? ('*/' | EOF)
  ;

fragment LineComment
  : '//' ~[\r\n]*
  ;

// --------------- Numbers ---------------

fragment DecimalConstant
  : '0'
  | [1-9] DecimalDigit*
  ;

fragment OctalConstant
  : '0' OctalDigit+
  ;

fragment HexadecimalConstant
  : '0' [xX] HexadecimalDigit+
  ;

fragment FractionalConstant
  : (DecimalDigit+ '.' DecimalDigit+)
  | DecimalDigit+ '.'
  | '.' DecimalDigit+
  ;

fragment ExponentPart
  : [eE] [+-] DecimalDigit+
  ;

fragment UnsingedIntegerSuffix
  : [uU]
  ;

fragment FloatingSuffix
  : [fF]
  ;

fragment DoubleSuffix
  : [dD]
  ;

// --------------- Digits ---------------

fragment DecimalDigit
  : [0-9]
  ;

fragment HexadecimalDigit
  : [0-9a-fA-F]
  ;

fragment OctalDigit
  : [0-7]
  ;

fragment NonDigit
  : [_a-zA-Z]
  ;
