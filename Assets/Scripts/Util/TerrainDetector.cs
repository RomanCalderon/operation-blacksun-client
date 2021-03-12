using UnityEngine;

public class TerrainDetector
{
    private TerrainData m_terrainData;
    private int m_alphamapWidth;
    private int m_alphamapHeight;
    private float [,,] m_splatmapData;
    private int m_numTextures;

    public TerrainDetector ()
    {
        m_terrainData = Terrain.activeTerrain.terrainData;
        m_alphamapWidth = m_terrainData.alphamapWidth;
        m_alphamapHeight = m_terrainData.alphamapHeight;

        m_splatmapData = m_terrainData.GetAlphamaps ( 0, 0, m_alphamapWidth, m_alphamapHeight );
        m_numTextures = m_splatmapData.Length / ( m_alphamapWidth * m_alphamapHeight );
    }

    public int GetActiveTerrainTextureIdx ( Vector3 position )
    {
        Vector3 terrainCord = ConvertToSplatMapCoordinate ( position );
        int activeTerrainIndex = 0;
        float largestOpacity = 0f;

        for ( int i = 0; i < m_numTextures; i++ )
        {
            if ( largestOpacity < m_splatmapData [ ( int ) terrainCord.z, ( int ) terrainCord.x, i ] )
            {
                activeTerrainIndex = i;
                largestOpacity = m_splatmapData [ ( int ) terrainCord.z, ( int ) terrainCord.x, i ];
            }
        }

        return activeTerrainIndex;
    }

    private Vector3 ConvertToSplatMapCoordinate ( Vector3 worldPosition )
    {
        Vector3 splatPosition = new Vector3 ();
        Terrain ter = Terrain.activeTerrain;
        Vector3 terPosition = ter.transform.position;
        splatPosition.x = ( ( worldPosition.x - terPosition.x ) / ter.terrainData.size.x ) * ter.terrainData.alphamapWidth;
        splatPosition.z = ( ( worldPosition.z - terPosition.z ) / ter.terrainData.size.z ) * ter.terrainData.alphamapHeight;
        return splatPosition;
    }
}