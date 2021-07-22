import { withApiAuthRequired, getAccessToken } from '@auth0/nextjs-auth0';
const https = require('https');

export default withApiAuthRequired(async function services(req, res) {
  try {

    console.log("inside withapi thing");

    var tokenResponse = await getAccessToken(req, res);

    console.log(tokenResponse.accessToken);

    // const httpsAgent = new https.Agent({
    //   rejectUnauthorized: false,
    // });

    const response = await fetch(process.env.IDENTITY_API + '/api/userprofile', {
      method: 'GET',
      headers: {
        authorization: `Bearer ${tokenResponse.accessToken}`
      },
      //agent: httpsAgent
    });


    const profileDetails = await response.json();
    res.status(response.status || 200).json(profileDetails);

  } catch (error) {
    console.error(error);
    res.status(error.status || 500).json({
      code: error.code,
      error: error.message
    });
  }
});
