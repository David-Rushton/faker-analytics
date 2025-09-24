# Faker Analytics

I created this project to develop a deeper understanding of the following:

- How to integrate generative AI into imperative applications
- How to provide generative AI with a series of tools to argument its capabilities
- How to extend a traditional application with generative AI features

My "traditional application" is called `faker-analytics`.  It is a collection of APIs, that provide
insights into the energy trading market.  All of the data is fake - hence the name.  The various APIs
cover only basic functionality, as building a true analytics system is not the aim here.

You can track my progress in the [blog](./blog/)

## Getting Started

The easiest way to start is by running this script.  If anything is missing from your local machine
the script will explain what is missing, and how to fix it.

```pwsh
# The name pretty much covers it.
./scripts/Test-Prerequisites.ps1
```

### Gemini API Key

You will need a Gemini API key.  These can be generated in Google AI Studio or Google Cloud.
Instructions for both can be found [here](https://ai.google.dev/gemini-api/docs/api-key).

### Docker 

You will need [Docker](https://docs.docker.com/engine/install/).

I haven't tested with Podman, Rancher or any of the other alternatives.  But in theory there is no
reason why they shouldn't work.
