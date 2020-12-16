module Aws

open Pulumi
open Pulumi.FSharp
open Pulumi.Aws.Iam
open Pulumi.Aws.Lambda

open Domain

let private lambdaRole = 
    Role("lambdaRole", 
        RoleArgs
           (AssumeRolePolicy = input
                @"{
                    ""Version"": ""2012-10-17"",
                    ""Statement"": [
                        {
                            ""Action"": ""sts:AssumeRole"",
                            ""Principal"": {
                                ""Service"": ""lambda.amazonaws.com""
                            },
                            ""Effect"": ""Allow"",
                            ""Sid"": """"
                        }
                    ]
                }"))

let logPolicy = 
    RolePolicy("lambdaLogPolicy", 
        RolePolicyArgs
           (Role = io lambdaRole.Id,
            Policy = input
                @"{
                    ""Version"": ""2012-10-17"",
                    ""Statement"": [{
                        ""Effect"": ""Allow"",
                        ""Action"": [
                            ""logs:CreateLogGroup"",
                            ""logs:CreateLogStream"",
                            ""logs:PutLogEvents""
                        ],
                        ""Resource"": ""arn:aws:logs:*:*:*""
                    }]
                }"))

let private makeAwsLambda (Person name) (r: Runtime) () =
    let lowerName = name.ToLowerInvariant()
    let runtime = 
        match r with
        | Node -> "nodejs12.x"
        | DotNet -> "dotnetcore2.1"
        | Python -> "python3.8"
        | Java -> "java11"
        | Go -> "go1.x"
        |> input

    let lambda =
        Function(lowerName,
            FunctionArgs
               (Runtime = runtime,
                Code = input (FileArchive("../DotnetLambda/src/DotnetLambda/bin/Debug/netcoreapp2.1/publish") :> Archive),
                Role = io lambdaRole.Arn))
    Url lambda.Arn

let aws (wish: Wish) =
    match wish.Resource with
    | Serverless s8s -> makeAwsLambda wish.Recipient s8s |> Some
    | _ -> None
