const { DataApi } = require("@unity-services/cloud-save-1.2");

const GLOBAL_PLAYER_ID = "yyZKcUEy3OhPkqMatYRM4yFL38wQ";

module.exports = async ({ params, context, logger }) => {

  const { projectId, playerId } = context;
  const { score } = params;

  const cloudSaveAPI = new DataApi(context);

  const result = await cloudSaveAPI.setItem(projectId, GLOBAL_PLAYER_ID, {
    key: "leaderboard",
    value: [
      {
        "name": "Renan",
        "score": 2
      },
      {
        "name": "Rodrigo",
        "score": 1
      },
      {
        "name": "Pelipe",
        "score": 0
      },
    ]
  });


  logger.info(`result = ${result.data}`);

  return result.data;
};

