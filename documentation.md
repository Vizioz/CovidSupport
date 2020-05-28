# CovidSupport project

## Umbraco Provider

The CovidSupport project is built using the Umbraco CMS. Umbraco can host multiple sites, each with its own domain. Users who own a website can log in to the Umbraco backoffice using their username and password and access the content of their website.

Each site inside Umbraco can be used as a stand alone website, making use of Umbraco capabilities. Developers are able to use their own views and assets to display a fully working website under the site's domain.

They can also be used as a "headless" or back-end only content management system, without any views or displaying, but used solely as a content repository for storing and managing content. The content is accessible via a RESTful API that is explained below.

## Users

Umbraco has the capability of allowing different types of users to log in to the backoffice, with different types of permissions.

- Administrator. This user has full control over all content of the different websites, as well as settings and other users' permissions.

- Website Owner. This type of user has access to the Content and Media sections inside Umbraco. Permissions to each particular user will be restricted to their own website.

## Content structure

Content in Umbraco CMS follows a hierarchical structure. Content nodes can be created as children of other content nodes, corresponding to the URL structure of the website pages. The type of content that can be created inside other nodes depends on the specific permissions for each content type. The different content types are defined in the Settings section by an administrator.

The only content type accepted in the root of the content tree is "Website" content. Each one of these root nodes represent an instance of a website managed by a Website Owner. As specified previously, Website Owners will be restricted to access only their own website content.

A root Website node contains generic settings for the website.

A root Website can contain a "Resources" type node. This node will contain all resources available for this website, and they will be structured according to a tree of categories and subcategories.

### Creating content

A Website Owner can create content inside their own website from the Content section in the Umbraco backoffice. To do this, position the mouse over the name of the content node in the content tree to which you wish to add any children. A three-dot icon will appear to the right of the node name. If click on it, you will get a menu with a list of possible nodes that are allowed under the specific node. Just click on the one you want to create and fill in the name and the rest of fields. Once you are done, you can click on the "Save and publish" button on the bottom right corner of the screen. If you would like to save your changes, but don't want to make the content publicly available, you can click on the "Save" button instead.

## RESTful API

The CovidSupport project offers a public RESTful API to retrieve the resources saved on the server.

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

An array of ResourceListItem elements.

A ResourceListItem element contains only basic information about the resource.

- _ResourceListItem_

  Parameter	| Type | Description
  --|--|--  
  address | String | The address of the resource. This field usually contains only the street and street number.
  city | String | The city where the resource is at.
  description | String | An optional short description of the resource assets, for instace, the type of cuisine.
  id | Integer | The ID of the resource.
  lat | Decimal | The geographical latitude.
  lon | Decimal | The geographical longitude.
  options | Array of String | All the options that are available for the resource, i.e. "delivery", "orderOnline", etc.
  providerName | String | The resource name.
  region | String | The region the resource is at.
  state | String | The code of the state, i.e. "NC".
  zip | String | The ZIP code of the address.

**Example of a JSON Response**

```JSON
[
  {
    "address": "1130 Blissenbach Ln",
    "city": "Chapel Hill",
    "description": null,
    "id": 2001,
    "lat": 35.911326,
    "lon": -79.218949,
    "options": [
      "orderOnline", "payOnline", "mustPreorder", "farmPickup", "delivery"
    ],
    "providerName": "Beechcrest Farm",
    "region": "Orange",
    "state": "NC",
    "zip": "27516"
  },
  {
    "address": "4115 Garret Dr",
    "city": "Durham",
    "description": null,
    "id": 2002,
    "lat": 36.011597,
    "lon": -79.027342,
    "options": [
      "orderOnline", "payOnline", "mustPreorder", "farmPickup", "delivery"
    ],
    "providerName": "Red's Quality Acre",
    "region": "Orange",
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

A resource element contains all the available information about the resource.

- _Resource_

  Parameter	| Type | Description
  --|--|--  
  address | String | The address of the resource. This field usually contains only the street and street number.
  city | String | The city where the resource is at.
  contact | String | The main telephone number.
  contactSpanish | String | The telephone number for Spanish-speaking customers.
  description | String | An optional short description of the resource assets, for instace, the type of cuisine.
  email | String | The contact email address.
  facebook | String | The Facebook page URL for the resource.
  free | Boolean | It indicates whether the resource services are offered free of charge.
  id | Integer | The ID of the resource.
  instagram | String | The Instagram username for the resource.
  instructions | String | Optional instruction.
  notes | String | Optional extra notes.
  offers | String | Optional offers.
  lat | Decimal | The geographical latitude.
  lon | Decimal | The geographical longitude.
  options | Array of String | All the options that are available for the resource, i.e. "delivery", "orderOnline", etc.
  providerAddLoc | String | Provider's secondary name. For example, for school meals this could be the local county department.
  providerName | String | The resource name.
  region | String | The region/county the resource is at.
  state | String | The code of the state, i.e. "NC".
  twitter | String | The Twitter username for the resource.
  webLink | String | The URL of the resource's website.
  zip | String | The ZIP code of the address.
  monday | String | Monday opening hours.
  tuesday | String | Tuesday opening hours.
  wednesday | String | Wednesday opening hours.
  thursday | String | Thursday opening hours.
  friday | String | Friday opening hours.
  saturday | String | Saturday opening hours.
  sunday | String | Sunday opening hours.
  spMonday | String | Monday opening hours for senior shopping.
  spTuesday | String | Tuesday opening hours for senior shopping.
  spWednesday | String | Wednesday opening hours for senior shopping.
  spThursday | String | Thursday opening hours for senior shopping.
  spFriday | String | Friday opening hours for senior shopping.
  spSaturday | String | Saturday opening hours for senior shopping.
  spSunday | String | Sunday opening hours for senior shopping.

**Example of a JSON Response**

```JSON
{
  "address": "1130 Blissenbach Ln",
  "city": "Chapel Hill",
  "contact": "919-000-0000",
  "contactSpanish": null,
  "description": null,
  "email": "email@mail.com",
  "facebook": "https://www.facebook.com/Beechcrest-Farm-123",
  "free": false,
  "friday": "9:00 AM-5:00 PM",
  "id": 2001,
  "instagram": "BeechcrestFarm",
  "instructions": null,
  "lat": 35.911326,
  "lon": -79.218949,
  "monday": "9:00 AM-5:00 PM",
  "notes": null,
  "offers": null,
  "options": [
    "orderOnline", "payOnline", "mustPreorder", "farmPickup", "delivery"
  ],
  "providerAddLoc": null,
  "providerName": "Beechcrest Farm",
  "region": "Orange",
  "saturday": "9:00 AM-5:00 PM",
  "spFriday": "9:00 AM-5:00 PM",
  "spMonday": "9:00 AM-5:00 PM",
  "spSaturday": "9:00 AM-5:00 PM",
  "spSunday": "9:00 AM-5:00 PM",
  "spThursday": "9:00 AM-5:00 PM",
  "spTuesday": "9:00 AM-5:00 PM",
  "spWednesday": "9:00 AM-5:00 PM",
  "state": "NC",
  "sunday": "9:00 AM-5:00 PM",
  "thursday": "9:00 AM-5:00 PM",
  "tuesday": "9:00 AM-5:00 PM",
  "twitter": "BeechcrestFarm",
  "webLink": "https://beechcrestfarm.com",
  "wednesday": "9:00 AM-5:00 PM",
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

The indexes include all nodes of the type "Resource", such as restaurants, farms etc. Given the versatility of Umbraco, new resource types can be created when needed, and added to the available resources that editors can create. When a new resource type is created, it needs to be allowed to be included in the Examine index. This needs to be done by rebuilding the index. An administrator will need to perform the operation from the Settings section of Umbraco. When the index is rebuilt, it will find the new resource type and allow it.
