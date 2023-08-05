grammar SPSL
  ;

options {
  language = CSharp;
}

@header {
using SPSL.Language.AST;
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
KEYWORD_LOCAL
  : 'local'
  ;
KEYWORD_GLOBAL
  : 'global'
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
KEYWORD_PERMUTE
  : 'permute'
  ;
KEYWORD_STREAM
  : 'stream'
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
  : 'vector2b'
  ;
TYPE_VECTOR2F
  : 'vector2f'
  ;
TYPE_VECTOR2I
  : 'vector2i'
  ;
TYPE_VECTOR2UI
  : 'vector2ui'
  ;
TYPE_VECTOR3B
  : 'vector3b'
  ;
TYPE_VECTOR3F
  : 'vector3f'
  ;
TYPE_VECTOR3I
  : 'vector3i'
  ;
TYPE_VECTOR3UI
  : 'vector3ui'
  ;
TYPE_VECTOR4B
  : 'vector4b'
  ;
TYPE_VECTOR4F
  : 'vector4f'
  ;
TYPE_VECTOR4I
  : 'vector4i'
  ;
TYPE_VECTOR4UI
  : 'vector4ui'
  ;
TYPE_MATRIX2F
  : 'matrix2f'
  ;
TYPE_MATRIX3F
  : 'matrix3f'
  ;
TYPE_MATRIX4F
  : 'matrix4f'
  ;
TYPE_MATRIX2X3F
  : 'matrix2x3f'
  ;
TYPE_MATRIX2X4F
  : 'matrix2x4f'
  ;
TYPE_MATRIX3X2F
  : 'matrix3x2f'
  ;
TYPE_MATRIX3X4F
  : 'matrix3x4f'
  ;
TYPE_MATRIX4X2F
  : 'matrix4x2f'
  ;
TYPE_MATRIX4X3F
  : 'matrix4x3f'
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
  : 'texture1d'
  ;
TYPE_TEXTURE2D
  : 'texture2d'
  ;
TYPE_TEXTURE1DARRAY
  : 'texture1dArray'
  ;
TYPE_TEXTURE2DARRAY
  : 'texture2dArray'
  ;
TYPE_TEXTURE3D
  : 'texture3d'
  ;
TYPE_CUBEMAP
  : 'cubemap'
  ;
TYPE_CUBEMAPARRAY
  : 'cubemapArray'
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
TOK_EXCLAMATION
  : '!'
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
OP_XOR
  : '^^'
  ;
OP_LSHIFT
  : '<<'
  ;
OP_RSHIFT
  : '>>'
  ;

/* ====================
 Parser rules
 */
shaderFile
  : Directives = directive* DOC_COMMENT* Namespace = namespaceDefinition? (DOC_COMMENT* useNamespaceDirective)* FileLevelDefinitions = fileLevelDefinition* EOF
  ;

materialFile
  : Directives = directive* DOC_COMMENT* Namespace = namespaceDefinition? (DOC_COMMENT* useNamespaceDirective)* Material = material EOF
  ;

namespaceDefinition
  : KEYWORD_NAMESPACE Name = namespacedTypeName TOK_SEMICOLON
  ;

namespacedTypeName
  : IDENTIFIER (TOK_NAMESPACE_SEPARATOR IDENTIFIER)*
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
  : (KEYWORD_STATIC {$IsStatic = true;})? KEYWORD_CONST Type = dataType Definition = variableDeclarationAssignment TOK_SEMICOLON
  ;

permutationVariableBool
  : KEYWORD_PERMUTATION TYPE_BOOL Identifier = basicExpression OP_ASSIGN Value = BoolLiteral TOK_SEMICOLON
  ;

permutationVariableEnum
  : KEYWORD_PERMUTATION KEYWORD_ENUM Identifier = basicExpression TOK_OPEN_BRACE IDENTIFIER (TOK_COMMA IDENTIFIER)* TOK_CLOSE_BRACE OP_ASSIGN Value = basicExpression TOK_SEMICOLON
  ;

permutationVariableInteger
  : KEYWORD_PERMUTATION TYPE_INT Identifier = basicExpression OP_ASSIGN Value = IntegerLiteral TOK_SEMICOLON
  ;

permutationVariable
  : permutationVariableBool
  | permutationVariableEnum
  | permutationVariableInteger
  ;

type
  : DOC_COMMENT* Definition = structDefinition TOK_OPEN_BRACE (DOC_COMMENT* structComponent)* TOK_CLOSE_BRACE # Struct
  | DOC_COMMENT* Definition = enumDefinition TOK_OPEN_BRACE (DOC_COMMENT* enumComponent)* TOK_CLOSE_BRACE     # Enum
  ;

structDefinition
  : KEYWORD_TYPE Name = IDENTIFIER KEYWORD_AS KEYWORD_STRUCT (KEYWORD_EXTENDS ExtendedType = namespacedTypeName)?
  ;

enumDefinition
  : KEYWORD_TYPE Name = IDENTIFIER KEYWORD_AS KEYWORD_ENUM
  ;

interface
  : DOC_COMMENT* Definition = interfaceDefinition TOK_OPEN_BRACE (DOC_COMMENT* functionHead TOK_SEMICOLON)* TOK_CLOSE_BRACE
  ;

interfaceDefinition
  : KEYWORD_INTERFACE Name = IDENTIFIER (KEYWORD_EXTENDS ExtendedInterfaces = interfacesList)?
  ;

interfacesList
  : namespacedTypeName (TOK_COMMA namespacedTypeName)*
  ;

shaderFragment
  : DOC_COMMENT* Definition = shaderFragmentDefinition TOK_OPEN_BRACE (
    DOC_COMMENT* (shaderMember | permutationVariable | shaderFunction | useFragmentDirective)
  )* TOK_CLOSE_BRACE
  ;

shaderFragmentDefinition
  : KEYWORD_FRAGMENT Name = IDENTIFIER (KEYWORD_EXTENDS ExtendedFragment = namespacedTypeName)? (
    KEYWORD_IMPLEMENTS ExtendedInterfaces = interfacesList
  )?
  ;

shader
  : DOC_COMMENT* Definition = shaderDefinition TOK_OPEN_BRACE (
    DOC_COMMENT* (shaderMember | shaderFunction | useFragmentDirective)
  )* TOK_CLOSE_BRACE
  ;

material
  : DOC_COMMENT* Definition = materialDefinition TOK_OPEN_BRACE (DOC_COMMENT* (materialMember | useFragmentDirective))* TOK_CLOSE_BRACE
  ;

stream
  locals[bool IsPartial]
  : DOC_COMMENT* KEYWORD_STREAM TOK_OPEN_BRACE (DOC_COMMENT* streamProperty)* TOK_CLOSE_BRACE
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
    Type = (KEYWORD_VERTEX | KEYWORD_PIXEL | KEYWORD_GEOMETRY | KEYWORD_HULL | KEYWORD_DOMAIN)
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
  ;

samplerState
  : TYPE_SAMPLER Name = IDENTIFIER OP_ASSIGN IDENTIFIER                                 # DefaultSamplerState
  | TYPE_SAMPLER Name = IDENTIFIER TOK_OPEN_BRACE samplerStateProperty+ TOK_CLOSE_BRACE # CustomSamplerState
  ;

samplerStateProperty
  : Property = IDENTIFIER OP_ASSIGN Value = primitiveExpression
  ;

materialMember
  : materialParams
  | materialState
  | type
  | shaderFunction
  | materialShaderUsage
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
  : annotation* Storage = 'coherent'? Access = ('readonly' | 'writeonly' | 'readwrite' | 'constant')? KEYWORD_BUFFER Name = IDENTIFIER TOK_OPEN_BRACE bufferComponent*
    TOK_CLOSE_BRACE # InPlaceStructuredBufferDefinition
  | annotation* Storage = 'coherent'? Access = ('readonly' | 'writeonly' | 'readwrite')? KEYWORD_BUFFER OP_LESSER_THAN Type = dataType OP_GREATER_THAN Name = IDENTIFIER
    TOK_SEMICOLON # TypedBufferDefinition
  ;

bufferComponent
  : annotation* Type = dataType Name = IDENTIFIER TOK_SEMICOLON
  ;

materialParamsComponent
  : annotation* Type = dataType Name = IDENTIFIER (OP_ASSIGN DefaultValue = initializationExpression)? TOK_SEMICOLON # MaterialValueParameter
  | permutationVariable                                                                                              # MaterialPermutationParameter
  ;

structComponent
  : annotation* Type = dataType Name = IDENTIFIER TOK_SEMICOLON # StructProperty
  | annotation* Function = function                             # StructFunction
  ;

enumComponent
  : Name = IDENTIFIER (OP_ASSIGN Value = constantExpression)? TOK_COMMA
  ;

// Inner function variables
variableDeclaration
  locals[bool IsConst]
  : (KEYWORD_CONST                                          {$IsConst = true;})? Type = dataType variableIdentity (TOK_COMMA variableIdentity)* # TypedVariableDeclaration
  | KEYWORD_VAR Declaration = variableDeclarationAssignment # UntypedVariableDeclaration
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
  : annotation* (KEYWORD_OVERRIDE                                                                                   {$IsOverride = true;})? Function = function # BasicShaderFunction
  | annotation* Name = IDENTIFIER TOK_OPEN_PAREN TOK_CLOSE_PAREN TOK_OPEN_BRACE Body = functionBody TOK_CLOSE_BRACE # ShaderConstructorFunction
  ;

function
  : Head = functionHead TOK_OPEN_BRACE Body = functionBody TOK_CLOSE_BRACE
  ;

functionHead
  : Type = dataType Name = IDENTIFIER Signature = functionSignature
  ;

functionSignature
  : TOK_OPEN_PAREN Arguments = argList? TOK_CLOSE_PAREN
  ;

argList
  : argDef (TOK_COMMA argDef)*
  ;

argDef
  : DOC_COMMENT* Flow = (KEYWORD_IN | KEYWORD_OUT | KEYWORD_INOUT | KEYWORD_CONST)? Type = dataType Name = IDENTIFIER (
    OP_ASSIGN DefaultValue = constantExpression
  )?
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
  : Target = (KEYWORD_THIS | KEYWORD_BASE) TOK_DOT Member = basicExpression
  ;

methodMemberReferenceExpression
  : Target = (KEYWORD_THIS | KEYWORD_BASE) TOK_DOT Member = invocationExpression
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
  : KEYWORD_CASE Expression = constantExpression TOK_COLON TOK_OPEN_BRACE? Statements = stayControlFlowStatement* leaveControlFlowStatement TOK_CLOSE_BRACE?
  ;

whileStatement
  : KEYWORD_WHILE TOK_OPEN_PAREN Expression = expressionStatement TOK_CLOSE_PAREN Block = statementBlock
  ;

forStatement
  : KEYWORD_FOR TOK_OPEN_PAREN Initialization = expressionStatement TOK_SEMICOLON Condition = expressionStatement TOK_SEMICOLON Iteration = expressionStatement TOK_CLOSE_PAREN
    Block = statementBlock
  ;

doWhileStatement
  : KEYWORD_DO Block = statementBlock KEYWORD_WHILE Expression = parenthesizedExpression
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
  | Operator = (OP_INCREMENT | OP_DECREMENT) Expression = assignableExpression                                                            # PrefixUnaryOperationExpression
  | TOK_EXCLAMATION Expression = expressionStatement                                                                                      # NegateOperationExpression
  | Operator = (OP_MINUS | OP_PLUS) Expression = expressionStatement                                                                      # SignedExpression
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
  : KEYWORD_RETURN Expression = expressionStatement TOK_SEMICOLON
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
  | Type = TYPE_VECTOR2I
  | Type = TYPE_VECTOR2UI
  | Type = TYPE_VECTOR3B
  | Type = TYPE_VECTOR3F
  | Type = TYPE_VECTOR3I
  | Type = TYPE_VECTOR3UI
  | Type = TYPE_VECTOR4B
  | Type = TYPE_VECTOR4F
  | Type = TYPE_VECTOR4I
  | Type = TYPE_VECTOR4UI
  | Type = TYPE_MATRIX2F
  | Type = TYPE_MATRIX3F
  | Type = TYPE_MATRIX4F
  | Type = TYPE_MATRIX2X3F
  | Type = TYPE_MATRIX2X4F
  | Type = TYPE_MATRIX3X2F
  | Type = TYPE_MATRIX3X4F
  | Type = TYPE_MATRIX4X2F
  | Type = TYPE_MATRIX4X3F
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
  : (OP_PLUS | OP_MINUS)? (FractionalConstant DoubleSuffix | DecimalDigit+ DoubleSuffix)
  ;

FloatLiteral
  : (OP_PLUS | OP_MINUS)? (
    FractionalConstant ExponentPart? FloatingSuffix?
    | DecimalDigit+ ExponentPart FloatingSuffix?
  )
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
  : ('e' | 'E') ('+' | '-') DecimalDigit+
  ;

fragment UnsingedIntegerSuffix
  : ('u' | 'U')
  ;

fragment FloatingSuffix
  : ('f' | 'F')
  ;

fragment DoubleSuffix
  : ('d' | 'D')
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
  : ('_' | [a-z] | [A-Z])
  ;
