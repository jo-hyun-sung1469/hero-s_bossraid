동굴 바닥 타일셋

구성
- cave_floor_tileset_4x4.png: 1024 x 1024 PNG, 4열 x 4행
- tiles/cave_floor_00.png ~ cave_floor_15.png: 각 256 x 256 PNG
- 현재 구성은 바닥 무늬 변형 16종입니다.

Unity에서 시트 사용
1. cave_floor_tileset_4x4.png를 Assets에 넣습니다.
2. Texture Type: Sprite (2D and UI)
3. Sprite Mode: Multiple
4. Pixels Per Unit: 256 (Grid 셀이 1 x 1인 경우)
5. Filter Mode: Point (no filter), Compression: None
6. Sprite Editor > Slice > Grid By Cell Count
7. Column: 4, Row: 4, Offset: 0, Padding: 0
8. Slice > Apply
9. 분할된 16개 스프라이트를 Tile Palette로 드래그합니다.

개별 PNG 사용
tiles 폴더의 PNG는 Sprite Mode: Single, Pixels Per Unit: 256으로 설정하여 사용합니다.

제작 안내
이미지 생성으로 만든 바닥 변형을 정규 크기로 내보내고 분할했습니다.
임의 순서로 배치할 때의 모든 경계 조합은 Unity에서 검증하지 않았습니다.
