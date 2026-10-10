# D51 â€” mimari maket gÃ¶rÃ¼nÃ¼mÃ¼ (2026-10-10)

KullanÄ±cÄ± beyaz kaliteli maket kartonu, kÃ¢ÄŸÄ±ttan kesilmiÅŸ aÄŸaÃ§lar, teknik Ã§izim
zemini ve Ã¶lÃ§Ã¼lÃ¼ bordo vurgular istedi. Referanslar sanat dilidir; trenin tÃ¼rÃ¼,
kapÄ± yerleri, geÃ§itleri ve oynanÄ±ÅŸ Ã¶lÃ§Ã¼leri deÄŸiÅŸmez. Bu ilk gÃ¶rsel denemedir;
kullanÄ±cÄ± kalite kabulÃ¼, reklam CTR artÄ±ÅŸÄ± veya mobil FPS garantisi deÄŸildir.

## Uygulanan dil

- D48 gÃ¶vdesi: mat beyaz/aÃ§Ä±k gri karton; koyu pencere Ã§erÃ§evesi ve alt takÄ±m.
- KapÄ± yapraklarÄ±: bordo boyalÄ± karton. AynÄ± portal gÃ¶rselini izler, kÄ±rÄ±lÄ±rken
  gizlenir ve onarÄ±mda geri gelir. Cam ve metallerin mevcut atÄ±ÅŸ kurallarÄ± kalÄ±r.
- KÃ¶rÃ¼k: aÃ§Ä±k gri kÄ±vrÄ±lmÄ±ÅŸ karton; aynÄ± iki adet 3m aÃ§Ä±k geÃ§it.
- Ray/travers: grafit maket parÃ§asÄ±. Ä°stasyon/yol: beyaz karton taban, Ã¶zgÃ¼n
  teknik plan Ã§izimleri, kesiÅŸen iki dÃ¼z siluetten yapÄ±lmÄ±ÅŸ karton aÄŸaÃ§lar.
- Ã‡evreye Ã¶zel ucuz derinlik koyulaÅŸmasÄ±; tren/oyuncu/dÃ¼ÅŸman okunabilirliÄŸini
  korur. Kamera takibi, FOV ve mobil kadraj hesabÄ± deÄŸiÅŸmez. UzaklarÄ±n kararmasÄ±
  bulanÄ±klÄ±k efekti veya tam ekran postprocess deÄŸildir.
- Oyuncu ve dÃ¼ÅŸman kapsÃ¼lleri ÅŸimdilik ayÄ±rt edilebilir yeÅŸil/kiremit tonlarda.
  Turret/dron/kutu iskeletleri sahneye Ã¶zel karton malzemeye geÃ§er; ateÅŸ,
  patlama ve mayÄ±n LED malzemeleri Ã§alÄ±ÅŸÄ±r. Karakter modeli bu teslim deÄŸildir.

## Teknik sÃ¶zleÅŸme

`Tools/TrainArt/build_game_maquette.py` eski `TrainMeshes.json` dosyasÄ±nÄ±
salt okunur alÄ±r. AÃ§Ä±klÄ±klar/ana yÃ¼zey konumlarÄ± aynÄ±. BÃ¼yÃ¼k zemin Ã¼Ã§genleri
aynÄ± dÃ¼zlemde bÃ¶lÃ¼nÃ¼r; 18 Ä±ÅŸÄ±nlÄ±, 0.65m eriÅŸimli offline AO kÃ¶ÅŸe renklerine
yazÄ±lÄ±r. Oyun AO hesaplamaz. ÃœÃ§ vagonun opak sanatÄ± 26.850 Ã¼Ã§gendir; camlar
eski meshtir. Bu sayÄ± bÃ¼tÃ¼n sahnenin veya tek karede gÃ¶rÃ¼nen Ã¼Ã§gen sayÄ±sÄ± deÄŸildir.

`WaitYourTurn/MaquetteCard` tek opak forward pass kullanÄ±r: kÃ¶ÅŸe rengi, mat
aydÄ±nlatma ve tek 64Ã—64 Ã¶zgÃ¼n kÃ¢ÄŸÄ±t grain haritasÄ±. Normal/metallic atlasÄ±,
realtime gÃ¶lge/probe/DOF/bloom/SSAO eklenmez. Android grain ASTC6Ã—6, mipmap,
Read/Write kapalÄ±; meshler UInt16 ve CPU Read/Write kapalÄ±dÄ±r. KÃ¼Ã§Ã¼k harita
minimum boyuttan dolayÄ± ASTC'de bir kaÃ§ KB'dÄ±r; kaynak JSON/Blender runtime
Resources iÃ§ine konmaz, sahne yalnÄ±z Ã¼retilmiÅŸ mesh/materyalleri referanslar.

Ä°stasyon ve tekrar eden Ã§evre parÃ§alarÄ± tek yaprak/Ã§izgi baÅŸÄ±na GameObject
yerine birleÅŸtirilmiÅŸ meshlerdir. StationCard/StationInk iki renderer; yol
SceneryCard/SceneryInk 20m ortak tile. 6 tile aynÄ± iki mesh'i paylaÅŸÄ±r. Tekrar
mesafesi mevcut RunPresentation'Ä±n20m periyoduyla eÅŸleÅŸir; bu sÄ±nÄ±f tek hareket
otoritesidir. Yeni collider/agent/obstacle/nav kaynaÄŸÄ± yoktur. Kaydedilmeden Ã¶nce
**tÃ¼m sahne collider/agent/obstacle verileri + transformlarÄ± ve NavMesh hash'i**
Ã¶nce/sonra karÅŸÄ±laÅŸtÄ±rÄ±lÄ±r. Battle sahnesi ve ortak iÃ§erik malzemeleri deÄŸiÅŸmez.

## Kaynak ve yeniden uygulama

- `ArtSource/GameMaquette/GameMaquette.blend`: oyun Ã¶lÃ§Ã¼sÃ¼ndeki dÃ¼zenlenebilir kaynak.
- `ArtSource/GameMaquette/manifest.json`: geometri bÃ¼tÃ§esi/kaynak hash'i.
- `Assets/Game/Art/Maquette/SourceData/MaquetteMeshes.json`: yalnÄ±z editor aktarÄ±mÄ±.
- **Wait Your Turn â†’ Survival â†’ Install Architectural Maquette** (Ctrl+Shift+Alt+M),
  Play durmuÅŸken, yalnÄ±z SurvivalIntegration. Tekrarlanabilir; mevcut mekanik
  gÃ¶vde nesneleri ve kimlikleri korunur, eski ortam rendererlarÄ± kapatÄ±lÄ±r.
- **Render Maquette Preview**: Unity oyun kamerasÄ±nda 1600Ã—900 gÃ¶rsel; geÃ§ici
  render target bÄ±rakÄ±lmaz, oyun kameranÄ±n aspect hesabÄ±na geri dÃ¶ner.
- `docs/generated/maquette-install.txt`: gerÃ§ek Unity kurulum kanÄ±tÄ±.
- `docs/generated/maquette-unity-preview.png`: Unity renderi, offline Blender
  sunum Ä±ÅŸÄ±ÄŸÄ± veya reklam iÃ§in vaat edilen gÃ¶rÃ¼nÃ¼m deÄŸildir. UI gÃ¶rsel dÄ±ÅŸÄ±nda.

Blender kaynak Ã¶nizlemesinde Cycles area Ä±ÅŸÄ±klarÄ± kullanÄ±lÄ±r; bunlar Unity'ye
taÅŸÄ±nmaz. `maquette-game-scale-preview.png` ilk ara Ã§alÄ±ÅŸmadÄ±r; gÃ¼ncel oyun
gÃ¶rÃ¼nÃ¼mÃ¼ iÃ§in Unity dosyasÄ±na bakÄ±lÄ±r. DiÄŸer konuÅŸmadaki ArchitecturalTrainScene
ve EmptyTrainModel dosyalarÄ± baÄŸÄ±msÄ±zdÄ±r; bu kurulum onlarÄ± deÄŸiÅŸtirmez.

Sonraki kontrol: kullanÄ±cÄ± gÃ¶rsel deÄŸerlendirmesi; uygun zamanda A54'te gerÃ§ek
kalabalÄ±k sahne FPS/Ä±sÄ±nma Ã¶lÃ§Ã¼mÃ¼. Ã–nceki blokout cihaz sonucu bu yeni sanata
otomatik taÅŸÄ±nmaz. Battle'a maket temasÄ± ancak ayrÄ± uygulama/kabul adÄ±mÄ±nda gelir.

## Bu turdaki kısa kabul

Unity derlemesi ve shader importu geçti. Kurulum bütün sahne collider/agent/
obstacle durum/transformlarını ve NavMesh hash'ini korudu. Play'de 15 kapı
cam/metal rayı, kırılma/görselin gizlenmesi/onarım/geri gelmesi, 15 yan cam
ve iki körükten fiziksel yürüyüş PASS. Kanıt
`docs/generated/maquette-survival-train-art-pc-acceptance.txt`; önceki D48
kanıtları üzerine yazılmadı. Play sonrasında Console hata/uyarı sayısı0.

8s Editor kare örneği: 1.602 kare, ortalama200,3FPS, P95=7,49ms,
en fazla4 canlı zombi; `maquette-pc-frame-sample.txt`. Editor ve mevcut
PC pencere çözünürlüğünü içerir; kalabalık Android veya eşit koşullu önce/sonra
performans karşılaştırması değildir. A54 FPS/ısınma/pil ölçümü bekliyor.

D52 sinematik devamı ve güncel PC yük incelemesi: [MAQUETTE_CINEMA.md](MAQUETTE_CINEMA.md). D51'in postprocess yok kaydı ilk denemeyi anlatır; güncel Survival sınırlı tilt-shift kullanır.
