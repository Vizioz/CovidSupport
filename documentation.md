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

Umbraco can store multilingual sites, making the content nodes multilingual too. Content has thus the potential of acceptiong different text/value variations for different languages when applicable.

The current available languages are English and Spanish. By common practice, different language versions are usually available by appending the language code to the main domain URL, i.e. wwww.mywebsite.com/es. Each domain for each different language of a website needs to be defined in the Umbraco backoffice.

### Creating content

A Website Owner can create content inside their own website from the Content section in the Umbraco backoffice. To do this, position the mouse over the name of the content node in the content tree to which you wish to add any children. A three-dot icon will appear to the right of the node name. If click on it, you will get a menu with a list of possible nodes that are allowed under the specific node. Just click on the one you want to create and fill in the name and the rest of fields. Once you are done, you can click on the "Save and publish" button on the bottom right corner of the screen. If you would like to save your changes, but don't want to make the content publicly available, you can click on the "Save" button instead.

## RESTful API

The CovidSupport project offers a public RESTful API to retrieve the resources saved on the server.

### URLs

Each website stored on the server has a unique URL such as **{domain}/api/v1**. For instance, if we are trying to get the resources from the site nccovid.vizioz.com,
the URL would be **https://nccovid.vizioz.com/api/v1/**. Please make sure you use the right **http** or **https** protocol to access the API.

#### Multilingual URLs

Please note that sites that are multilingual will use a different URL to retrieve content for each language, corresponding to the domain defined for each language. For instance:

- **https://nccovid.vizioz.com/api/v1/** (English)
- **https://nccovid.vizioz.com/es/api/v1/** (Spanish)


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
  ]
}
```

### Get

``GET /api/v1/resource/get/{id} ``

It returns a single published resource by its ID.

**Response**

A single Resource element.

A resource element contains all the available information about the resource.

The resources returned by the API might be of different types (according to their category), so some resources might contain different properties or parameters. 

Below are detailed the two types of resource currently returned by the API. The parameters marked as bold in the list below are common to all types of resources.

- _Resource_

  Parameter	| Type | Description | Common to all resources
  --|--|--|--
  **address** | String | The address of the resource. This field usually contains only the street and street number. | YES
  **category** | String | The category of the resource. | YES
  **city** | String | The city where the resource is at. | YES
  **contact** | String | The main contact telephone number. | YES
  contactSpanish | String | The telephone number for Spanish-speaking customers. |
  **description** | String | An optional short description of the resource assets, for instace, the type of cuisine. | YES
  **email** | String | The contact email address. | YES
  **facebook** | String | The Facebook page URL for the resource. | YES
  **free** | Boolean | It indicates whether the resource services are offered free of charge. | YES
  **id** | Integer | The ID of the resource. | YES
  **instagram** | String | The Instagram username for the resource. | YES
  instructions | String | Optional instruction. |
  lat | Decimal | The geographical latitude. | 
  lon | Decimal | The geographical longitude. | 
  notes | String | Optional extra notes. |
  offers | String | Optional offers. |
  options | Array[String] | All the options that are available for the resource, i.e. "delivery", "orderOnline", etc. |
  providerAddLoc | String | Provider's secondary name. For example, for school meals this could be the local county department. |
  **name** | String | The resource name. | YES
  **openHours** | Array[OpeningTimes] | The opening hours of the resource, as an array of OpenHour object for each day of the week. | YES
  **region** | Array[String] | The region/county the resource is at, or the region(s) where it provides its services. | YES 
  specialHours | Array[OpeningTimes] | The special opening hours of the resource, as an array of OpenHour object for each day of the week. | 
  **state** | String | The code of the state, i.e. "NC". | YES
  **twitter** | String | The Twitter username for the resource. | YES
  **webLink** | String | The URL of the resource's website. | YES
  **zip** | String | The ZIP code of the address. | YES

- _SocialServiceResource_

  Parameter	| Type | Description | Common to all resources
  --|--|--|--
  acceptsMedicaid | Boolean | It indicates whether it accetps patients using MedicAid insurance. | 
  acceptsMedicare | Boolean | It indicates whether it accepts patients using MediCare insurance | 
  acceptsUninsuredPatients | Boolean | It indicates whether it accepts uninsured patients. | 
  **address** | String | The address of the resource. This field usually contains only the street and street number. | YES
  **category** | String | The category of the resource. | YES
  afterHoursPhone | String | Optional after-hour phone number. | 
  afterHoursPhoneInstructions | String | Specific instructions for using the after-hours phone number. | 
  **city** | String | The city where the resource is at. | YES
  **contact** | String | The main contact telephone number. | YES
  crisisPhone | String | Optional phone number for crisis or emergency situations. | 
  crisisPhoneInstructions | String | Specific instructions for using the crisis phone number. | 
  **description** | String | An optional short description of the resource assets. | YES
  eligibility | String | The eligibility of the service. | 
  **email** | String | The contact email address. | YES
  **facebook** | String | The Facebook page URL for the resource. | YES
  **free** | Boolean | It indicates whether the resource services are offered free of charge. | YES
  resourceAccessNotes | String | Instructions about how to gain access to this resource. | 
  **id** | Integer | The ID of the resource. | YES
  **instagram** | String | The Instagram username for the resource. | YES
  geographicalRestrictions | String | Optional field indicating any additional restrictions beyond the service regions | 
  holidayOpeningTimes | String | The opening times during holidays. | 
  languagePhones | Array[LanguagePhone] | Alternative phone numbers for different languages. | 
  languagesSupported | Array[String] | A list of the languages supported by the service | 
  lowCost | Boolean | It indicates whether it is a low cost service. | 
  **name** | String | The resource name. | YES
  **openHours** | Array[OpeningTimes] | The opening hours of the resource, as an array of OpenHour object for each day of the week. | YES
  **region** | Array[String] | The region/county the resource is at, or the region(s) where it provides its services. | YES 
  serviceProviderName | String | The name of the service provider. | 
  specialHoursOpeningTimes | String | The special opening hours. | 
  status | String | The status of the service during COVID, regarding the opening hours. | 
  resourceAccessNotes | String | Instructions about how to gain access to this resource. | 
  safeForUndocumentedIndividuals | Boolean | It indicates whether the resource services are safe for undocumented individuals. | 
  **state** | String | The code of the state, i.e. "NC". | YES
  tags | Array[String] | The list of tags or categories for the service. | 
  **twitter** | String | The Twitter username for the resource. | YES
  **webLink** | String | The URL of the resource's website. | YES
  **zip** | String | The ZIP code of the address. | YES

  - _OpeningTimes_

  Parameter	| Type | Description
  --|--|--
  day | String | The name of the day of the week.
  hours | Array[StartEndTime] | An array of multiple values or periods of time during the day, each one containing a start and and end time.

  - _StartEndTime_

  Parameter	| Type | Description
  --|--|--
  startTime | String | The start time, if any, of a period of the day.
  endTime | String | The end time, if any, of a period of the day.

  - LanguagePhone

  Parameter	| Type | Description
  --|--|--
  language | String | The name of the language.
  phoneNumber | String | The phone number for the specific language.

**Example of a JSON Response**

```JSON
{
  "address": "123 Farm Ln",
  "category": "Farm",
  "city": "Chapel Hill",
  "contact": "919-000-0000",
  "contactSpanish": null,
  "description": null,
  "email": "email@mail.com",
  "facebook": "https://www.facebook.com/farm",
  "free": false,
  "id": 2001,
  "instagram": "farm",
  "instructions": null,
  "lat": 35.911326,
  "lon": -79.218949,
  "name": "Farm",
  "notes": null,
  "offers": null,
  "openHours": [
    { 
      "day": "monday", 
      "hours": [ 
        {"startTime": "08:00:00", "endTime": "14:00:00"},
        {"startTime": "16:00:00", "endTime": "20:00:00"}
      ]
    },
    { 
      "day": "tuesday", 
      "hours": [ 
        {"startTime": "08:00:00", "endTime": "14:00:00"},
        {"startTime": "16:00:00", "endTime": "20:00:00"}
      ]
    },
    { 
      "day": "wednesday", 
      "hours": [ 
        {"startTime": "08:00:00", "endTime": "14:00:00"},
        {"startTime": "16:00:00", "endTime": "20:00:00"}
      ]
    },
    { 
      "day": "thursday", 
      "hours": [ 
        {"startTime": "08:00:00", "endTime": "14:00:00"},
        {"startTime": "16:00:00", "endTime": "20:00:00"}
      ]
    },
    { 
      "day": "friday", 
      "hours": [ 
        {"startTime": "08:00:00", "endTime": "14:00:00"},
        {"startTime": "16:00:00", "endTime": "20:00:00"}
      ]
    },
    { 
      "day": "saturday", 
      "hours": [ 
        {"startTime": "12:00:00", "endTime": "16:00:00"}
      ]
    }
  ],
  "options": [
    "orderOnline", "payOnline", "mustPreorder", "farmPickup", "delivery"
  ],
  "providerAddLoc": null,
  "region": ["Orange"],
  "specialHours": [],
  "state": "NC",
  "twitter": "farm",
  "webLink": "https://www.website.com",
  "zip": "27516"
}
```

### Get By Category

``GET /api/v1/resource/getbycategory/{categoryId} ``

It returns the list of all published resources under a specific category in the category tree.

**Response**

An array of ResourceListItem elements.

A ResourceListItem element contains only basic information about the resource.

The resources returned by the API might be of different types (according to their category), so some resources might contain different properties or parameters. 

Below are detailed the two types of resource currently returned by the API. The parameters marked as bold in the list below are common to all types of resources.

- _ResourceListItem_

  Parameter	| Type | Description | Common to all resources
  --|--|--|--
  **address** | String | The address of the resource. This field usually contains only the street and street number. | YES
  **category** | String | The category of the resource. | YES
  **city** | String | The city where the resource is at. | YES
  **description** | String | An optional short description of the resource assets, for instace, the type of cuisine. | YES
  **id** | Integer | The ID of the resource. | YES
  lat | Decimal | The geographical latitude. | 
  lon | Decimal | The geographical longitude. | 
  **name** | String | The resource name. | YES
  options | Array[String] | All the options that are available for the resource, i.e. "delivery", "orderOnline", etc. |
  **region** | Array[String] | The region/county the resource is at, or the region(s) where it provides its services. | YES 
  **state** | String | The code of the state, i.e. "NC". | YES
  **zip** | String | The ZIP code of the address. | YES

- _SocialServiceResourceListItem_

  Parameter	| Type | Description | Common to all resources
  --|--|--|--
  **address** | String | The address of the resource. This field usually contains only the street and street number. | YES
  **category** | String | The category of the resource. | YES
  **city** | String | The city where the resource is at. | YES
  **description** | String | An optional short description of the resource assets, for instace, the type of cuisine. | YES
  **id** | Integer | The ID of the resource. | YES
  **name** | String | The resource name. | YES
  **region** | Array[String] | The region/county the resource is at, or the region(s) where it provides its services. | YES 
  **state** | String | The code of the state, i.e. "NC". | YES
  tags | Array[String] | The list of tags or categories for the service. |
  **zip** | String | The ZIP code of the address. | YES

**Example of a JSON Response**

```JSON
[
  {
    "address": "123 Farm Ln",
    "category": "Farm",
    "city": "Chapel Hill",
    "description": null,
    "id": 2001,
    "lat": 35.911326,
    "lon": -79.218949,
    "name": "Farm",
    "options": [
      "orderOnline", "payOnline", "mustPreorder", "farmPickup", "delivery"
    ],
    "region": "Orange",
    "state": "NC",
    "zip": "27516"
  },
  {
    "address": "123 Fake Dr",
    "category": "Farm",
    "city": "Durham",
    "description": null,
    "id": 2002,
    "lat": 36.011597,
    "lon": -79.027342,
    "name": "Farm 2",
    "options": [
      "mustPreorder", "farmPickup", "delivery"
    ],
    "region": "Orange",
    "state": "NC",
    "zip": "27705"
  }
]
```

### Get By Region

``GET /api/v1/resource/getbyregion/{regionId} ``

It returns the list of all published resources that are located or serve a specific region.

**Response**

An array of ResourceListItem elements.

_See Response for **Get By Category**_

## Resource indexes

Internally, the resource search queries are processed by Umbraco Examine, which implements a .NET search engine on the Umbraco nodes data. The Examine indexes are created dinamically and include all nodes published under each "Resources" node. Each website contains and searches its own index.

The indexes include all nodes of the type "Resource", such as restaurants, farms etc. Given the versatility of Umbraco, new resource types can be created when needed, and added to the available resources that editors can create. When a new resource type is created, it needs to be allowed to be included in the Examine index. This needs to be done by rebuilding the index. An administrator will need to perform the operation from the Settings section of Umbraco. When the index is rebuilt, it will find the new resource type and allow it.
