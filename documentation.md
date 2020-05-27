# CovidSupport project

## Provider

The CovidSupport project is built using the Umbraco CMS.

## Content structure

## API documentation

The CovidSupport project offers a public REST API to retrieve the resources saved on the server.

### URLs

Each website stored on the server has a unique URL such as **{domain}/api/v1**. For instance, if we are trying to get the resources from the site covidsupport.vizioztest.site,
the URL would be **https://covidsupport.vizioztest.site/api/v1/**. Please make sure you use the right **http** or **https** protocol to access the API.

### Public access

Each website API is publicly accessible. There is no need to use any credentials or authorization. The API contains only ``GET`` methods. Users can only retrieve data; they cannot add or edit any data stored in the server. Future methods to upload or update existing entries will be protected.

### Data response

Successful calls to the API will return data in JSON format. Examples of the specific data returned by the different API endpoints are explained below.

### API endpoints

#### Settings

``GET /api/v1/resource/settings``

It returns a list of the available resource categories (i.e. Restaurants, Farms & Farmers' Markets, etc), and a list of all available regions.

**Response**

Parameter	| Type | Description
--|--|--  
categories | Array of Category elements | The list of availabe categories, organized in a hierarchical tree structure of categories and subcategories. Each Category element
regions | Array of strings | The list of regions.

- _Category_

  Parameter	| Type | Description
  --|--|--  
  code | String | The code to identify this specific category.
  id | Integer | The ID of the category.
  name | string | The name of the category.
  subcategories | Array of Category elements | The list of available subcategories contained under this specific category. Each subcategory can contain itself another list of subcategories.

**Example of a JSON Response**

```JSON
{
	"categories": [
    {
      "code": "food",
      "id": 1001,
      "name": "Food Resources",
      "subcategories": [
        {
          "code": "restaurants",
          "id": 1002,
          "name": "Restaurants"
        },
        {
          "code": "specialtyFoodBeverage",
          "id": 1003,
          "name": "Specialty Food & Beverage"
        }
      ]
    },
    {
      "code": "farms",
      "id": 1004,
      "name": "Farms & Farm Markets"
    }
  ],
  "regions": [
    "Orange"
  ]
}
```

### Get All

``GET /api/v1/resource/getall ``

It returns the list of all published resources.

**Response**

An array of Resource elements.

- _Resource_

  Parameter	| Type | Description
  --|--|--  
  address | String | The address of the resource. This field usually contains only the street and street number.
  city | String | The city where the resource is at.
  description | String | An optional short description of the resource assets, for instace, the type of cuisine.
  fullAddress | String | The full address, including city, state and ZIP code.
  id | Integer | The ID of the resource.
  lat | Decimal | The geographical latitude.
  lon | Decimal | The geographical longitude.
  options | Array of String | All the options that are available for the resource, i.e. "delivery", "orderOnline", etc.
  providerName | String | The resource name.
  region | String | The region the resource is at.
  shortAddress | String | The address, including street, street number and city, but not the state or ZIP code.
  state | String | The code of the state, i.e. "NC".
  zip | String | The ZIP code of the address.

**Example of a JSON Response**

```JSON
[
  {
    "address": "1130 Blissenbach Ln",
    "city": "Chapel Hill",
    "description": null,
    "fullAddress": "1130 Blissenbach Ln, Chapel Hill, NC 27516",
    "id": 2001,
    "lat": 35.911326,
    "lon": -79.218949,
    "options": [
      "orderOnline", "payOnline", "mustPreorder", "farmPickup", "delivery"
    ],
    "providerName": "Beechcrest Farm",
    "region": "Orange",
    "shortAddress": "1130 Blissenbach Ln, Chapel Hill",
    "state": "NC",
    "zip": "27516"
  },
  {
    "address": "4115 Garret Dr",
    "city": "Durham",
    "description": null,
    "fullAddress": "4115 Garret Dr, Durham, NC 27705",
    "id": 2002,
    "lat": 36.011597,
    "lon": -79.027342,
    "options": [
      "orderOnline", "payOnline", "mustPreorder", "farmPickup", "delivery"
    ],
    "providerName": "Red's Quality Acre",
    "region": "Orange",
    "shortAddress": "4115 Garret Dr, Durham",
    "state": "NC",
    "zip": "27705"
  }
]
```

### Get

``GET /api/v1/resource/get/{id} ``

It returns a single published resource by its ID.

**Response**

A single Resource element.

**Example of a JSON Response**

```JSON
{
  "address": "1130 Blissenbach Ln",
  "city": "Chapel Hill",
  "description": null,
  "fullAddress": "1130 Blissenbach Ln, Chapel Hill, NC 27516",
  "id": 2001,
  "lat": 35.911326,
  "lon": -79.218949,
  "options": [
    "orderOnline", "payOnline", "mustPreorder", "farmPickup", "delivery"
  ],
  "providerName": "Beechcrest Farm",
  "region": "Orange",
  "shortAddress": "1130 Blissenbach Ln, Chapel Hill",
  "state": "NC",
  "zip": "27516"
}
```

### Get By Category

``GET /api/v1/resource/getbycategory/{categoryCode} ``

It returns the list of all published resources under a specific category in the category tree.

**Response**

An array of Resource elements.

**Example of a JSON Response**

_See JSON response for **Get All**_

## Resource indexes

Internally, the resource search queries are processed by Umbraco Examine, which implements a .NET search engine on the Umbraco nodes data. The Examine indexes are created dinamically and include all nodes published under each "Resources" node. Each website contains and searches its own index.

The indexes include all nodes of the type "Resource", such as restaurants, farms etc. Given the versatility of Umbraco, new resource types can be created when needed, and added to the available resources that editors can create. Since the new resource type was not allowed to be included in the Examine index, it will be necessary to rebuild it. This can be done from the Settings section of Umbraco by an administrator with the right permissions. When the index is rebuilt, it will find the new resource type and allow it.
