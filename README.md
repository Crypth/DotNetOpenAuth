Theres a few additions which does NOT follow the OAuth2 Spec in this fork,
but does not affect any other parts of the implementation than the LinkedIn (oauth2) authentication.
In specific the httpmessagehandler that handles LinkedIn queryparam auth.
The LinkedIn part of their graph api is not fully implemented, but most profile, basic and extended, are there.
There's also a fix for the facebook specific expire time of the token.
